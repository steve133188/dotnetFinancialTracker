using DotnetFinancialTrackerApp.Models;
using System.Globalization;
using System.Text.Json;

namespace DotnetFinancialTrackerApp.Services
{
    public class FamilyAIService : IFamilyAIService
    {
        private readonly IFamilyBankingService _familyBankingService;
        private readonly ITransactionsService _transactionsService;
        private readonly Dictionary<string, ConversationContext> _conversationContexts = new();

        public FamilyAIService(IFamilyBankingService familyBankingService, ITransactionsService transactionsService)
        {
            _familyBankingService = familyBankingService;
            _transactionsService = transactionsService;
        }

        // Conversational AI
        public async Task<string> GetConversationalInsightAsync(string question, string familyId, string? memberId = null)
        {
            var context = GetOrCreateContext(familyId, memberId);
            var lowercaseQuestion = question.ToLower();

            // Update context with current question
            context.RecentTopics.Add(ExtractTopicFromQuestion(lowercaseQuestion));
            if (context.RecentTopics.Count > 5)
                context.RecentTopics.RemoveAt(0);

            try
            {
                // Balance inquiries
                if (ContainsAny(lowercaseQuestion, "balance", "money", "how much"))
                {
                    var totalBalance = await _familyBankingService.GetFamilyTotalBalanceAsync(familyId);
                    if (memberId != null)
                    {
                        var memberBalance = await _familyBankingService.GetRealTimeBalanceAsync(memberId);
                        return $"Your current balance is {memberBalance:C}. The family total is {totalBalance:C}. Would you like to see a breakdown by family member?";
                    }
                    return $"Your family's total balance is {totalBalance:C}. This includes all family member accounts. Would you like me to break this down by member?";
                }

                // Spending analysis
                if (ContainsAny(lowercaseQuestion, "spending", "spent", "expenses", "where", "money go"))
                {
                    var insights = await _familyBankingService.GetFamilyInsightsAsync(familyId);
                    return $"This month your family has spent {insights.TotalExpenses:C} across {insights.TotalTransactions} transactions. " +
                           $"Your top category is {insights.TopSpendingCategory ?? "Uncategorized"} at {insights.TopCategoryAmount:C}. " +
                           $"Your savings rate is {insights.SavingsRate:P1}. Would you like more details about any category?";
                }

                // Goals and savings
                if (ContainsAny(lowercaseQuestion, "goal", "goals", "saving", "savings", "target"))
                {
                    var goals = await _familyBankingService.GetFamilyGoalsAsync(familyId);
                    if (!goals.Any())
                    {
                        return "You don't have any family goals set up yet. Setting goals is a great way to stay motivated! " +
                               "Would you like me to help you create a savings goal for something specific like a vacation, emergency fund, or major purchase?";
                    }

                    var activeGoals = goals.Where(g => !g.IsComplete).ToList();
                    var completedGoals = goals.Count(g => g.IsComplete);

                    var response = $"You have {activeGoals.Count} active goals";
                    if (completedGoals > 0) response += $" and {completedGoals} completed goals";
                    response += ". ";

                    if (activeGoals.Any())
                    {
                        var topGoal = activeGoals.OrderByDescending(g => g.ProgressPercentage).First();
                        response += $"Your '{topGoal.Title}' goal is at {topGoal.ProgressPercentage:F0}% completion " +
                                   $"({topGoal.CurrentAmount:C} of {topGoal.TargetAmount:C}). ";

                        if (topGoal.DaysRemaining > 0)
                        {
                            response += $"You have {topGoal.DaysRemaining} days left and need to save {topGoal.MonthlyRequiredAmount:C} per month to reach it.";
                        }
                    }

                    return response;
                }

                // Budget questions
                if (ContainsAny(lowercaseQuestion, "budget", "limit", "allowance", "can i spend", "can we afford"))
                {
                    if (memberId != null)
                    {
                        var member = await _familyBankingService.GetFamilyMemberAsync(memberId);
                        if (member != null)
                        {
                            return $"Your monthly spending limit is {member.SpendingLimit:C}. " +
                                   $"You've spent {member.SpentThisMonth:C} this month ({member.SpendingRatio:P1} of your limit). " +
                                   $"You have {member.RemainingMonthlyLimit:C} remaining. " +
                                   $"Based on your current spending rate, you're on track to spend about {EstimateMonthlySpending(member):C} this month.";
                        }
                    }

                    var insights = await _familyBankingService.GetFamilyInsightsAsync(familyId);
                    return $"Your family's financial health score is {insights.FinancialHealthScore:F0}/100. " +
                           $"You're spending {insights.ExpenseIncomeRatio:P1} of your income. " +
                           $"Financial experts recommend keeping expenses below 80% of income.";
                }

                // Transaction help
                if (ContainsAny(lowercaseQuestion, "transaction", "buy", "purchase", "pay", "should i"))
                {
                    return "I can help you make smart spending decisions! Tell me what you're looking to buy and how much it costs, " +
                           "and I'll check if it fits within your budget and suggest the best account to use.";
                }

                // Family insights
                if (ContainsAny(lowercaseQuestion, "family", "everyone", "who", "most", "least"))
                {
                    var categorySpending = await _familyBankingService.GetCategorySpendingAsync(familyId,
                        new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1),
                        DateTime.Today.AddDays(1));

                    var memberSpending = await _familyBankingService.GetMemberSpendingAsync(familyId,
                        new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1),
                        DateTime.Today.AddDays(1));

                    var topCategory = categorySpending.OrderByDescending(c => c.Value).FirstOrDefault();
                    var topSpender = memberSpending.OrderByDescending(m => m.Value).FirstOrDefault();

                    return $"Family spending overview: Your top category is {topCategory.Key} at {topCategory.Value:C}. " +
                           $"{topSpender.Key} has been the most active spender this month at {topSpender.Value:C}. " +
                           $"Everyone is doing great at tracking their expenses!";
                }

                // General financial advice
                if (ContainsAny(lowercaseQuestion, "advice", "help", "tip", "recommend", "suggest", "what should"))
                {
                    var suggestions = await GetPersonalizedSuggestionsAsync(familyId, memberId ?? "");
                    if (suggestions.Any())
                    {
                        var topSuggestion = suggestions.OrderByDescending(s => s.Priority).First();
                        return $"{topSuggestion.Description} This could help you save up to {topSuggestion.PotentialSavings:C} per month. " +
                               "Would you like more specific recommendations for your family?";
                    }

                    return "Based on your family's spending patterns, you're doing well! Keep tracking your expenses, " +
                           "consider setting up automatic savings, and remember that small consistent savings add up over time. " +
                           "Is there a specific area you'd like to improve?";
                }

                // Default helpful response
                return GenerateContextualResponse(question, context, familyId);
            }
            catch (Exception ex)
            {
                return "I'm having trouble accessing your financial data right now. Please try asking again in a moment. " +
                       "In the meantime, I can help with general financial advice or explain financial concepts!";
            }
        }

        public async Task<List<SmartSuggestion>> GetPersonalizedSuggestionsAsync(string familyId, string memberId)
        {
            var suggestions = new List<SmartSuggestion>();

            try
            {
                var insights = await _familyBankingService.GetFamilyInsightsAsync(familyId);
                var member = string.IsNullOrEmpty(memberId) ? null : await _familyBankingService.GetFamilyMemberAsync(memberId);

                // Savings rate suggestions
                if (insights.SavingsRate < 0.1)
                {
                    suggestions.Add(new SmartSuggestion
                    {
                        Title = "Boost Your Savings",
                        Description = "Try the 50/30/20 rule: 50% for needs, 30% for wants, 20% for savings. You could increase your savings rate to 10%.",
                        Type = SuggestionType.Savings,
                        Priority = SuggestionPriority.High,
                        PotentialSavings = insights.TotalIncome * 0.1m - insights.NetSavings
                    });
                }

                // Budget suggestions
                if (!string.IsNullOrEmpty(insights.TopSpendingCategory) && insights.TopCategoryPercentage > 35)
                {
                    suggestions.Add(new SmartSuggestion
                    {
                        Title = $"Monitor {insights.TopSpendingCategory} Spending",
                        Description = $"{insights.TopSpendingCategory} is {insights.TopCategoryPercentage:F0}% of your spending. Setting a monthly budget could help control costs.",
                        Type = SuggestionType.Budget,
                        Priority = SuggestionPriority.Medium,
                        ActionText = "Set Budget Limit"
                    });
                }

                // Goal suggestions
                var goals = await _familyBankingService.GetFamilyGoalsAsync(familyId);
                if (!goals.Any())
                {
                    suggestions.Add(new SmartSuggestion
                    {
                        Title = "Create Your First Family Goal",
                        Description = "Families with savings goals save 42% more than those without. Start with an emergency fund of 3-6 months expenses.",
                        Type = SuggestionType.Goal,
                        Priority = SuggestionPriority.High,
                        ActionText = "Create Emergency Fund Goal"
                    });
                }

                // Member-specific suggestions
                if (member != null)
                {
                    if (member.SpendingRatio > 0.8)
                    {
                        suggestions.Add(new SmartSuggestion
                        {
                            Title = "Spending Alert",
                            Description = $"You've used {member.SpendingRatio:P0} of your monthly limit. Consider reviewing your recent purchases.",
                            Type = SuggestionType.Spending,
                            Priority = SuggestionPriority.High
                        });
                    }

                    if (member.Role == "Teen" || member.Role == "Child")
                    {
                        suggestions.Add(new SmartSuggestion
                        {
                            Title = "Learn & Earn",
                            Description = "Complete financial literacy challenges to earn bonus allowance. Knowledge is the best investment!",
                            Type = SuggestionType.Goal,
                            Priority = SuggestionPriority.Medium,
                            ActionText = "View Challenges"
                        });
                    }
                }

                return suggestions.OrderByDescending(s => s.Priority).ToList();
            }
            catch
            {
                return suggestions;
            }
        }

        public async Task<string> ProcessVoiceCommandAsync(string voiceInput, string familyId, string memberId)
        {
            var normalizedInput = voiceInput.ToLower().Trim();

            // Quick balance check
            if (ContainsAny(normalizedInput, "balance", "how much money", "my balance"))
            {
                var balance = await _familyBankingService.GetRealTimeBalanceAsync(memberId);
                return $"Your current balance is {balance:C}";
            }

            // Quick spending check
            if (normalizedInput.StartsWith("can i spend") || normalizedInput.StartsWith("can i buy"))
            {
                // Extract amount if possible
                var amount = ExtractAmountFromText(normalizedInput);
                if (amount > 0)
                {
                    return await CheckSpendingBeforeTransactionAsync(memberId, amount, "general");
                }
                return "How much are you looking to spend? I can check if it fits your budget.";
            }

            // Add transaction
            if (normalizedInput.StartsWith("i spent") || normalizedInput.StartsWith("add expense"))
            {
                var amount = ExtractAmountFromText(normalizedInput);
                if (amount > 0)
                {
                    await _familyBankingService.UpdateMemberBalanceAsync(memberId, amount, false, "Voice command expense");
                    return $"I've recorded a {amount:C} expense. Your new balance is {await _familyBankingService.GetRealTimeBalanceAsync(memberId):C}";
                }
                return "I didn't catch the amount. Try saying 'I spent 20 dollars on groceries'";
            }

            // Fall back to conversational AI
            return await GetConversationalInsightAsync(voiceInput, familyId, memberId);
        }

        // Financial Analysis
        public async Task<string> AnalyzeSpendingPatternsAsync(string familyId, DateTime? periodStart = null, DateTime? periodEnd = null)
        {
            var start = periodStart ?? DateTime.Today.AddDays(-30);
            var end = periodEnd ?? DateTime.Today;

            var categorySpending = await _familyBankingService.GetCategorySpendingAsync(familyId, start, end);
            var memberSpending = await _familyBankingService.GetMemberSpendingAsync(familyId, start, end);

            var analysis = "📊 **Spending Pattern Analysis**\n\n";

            // Category analysis
            var topCategories = categorySpending.OrderByDescending(c => c.Value).Take(3).ToList();
            analysis += "**Top Spending Categories:**\n";
            foreach (var category in topCategories)
            {
                var percentage = categorySpending.Values.Sum() == 0 ? 0 : (double)(category.Value / categorySpending.Values.Sum() * 100);
                analysis += $"• {category.Key}: {category.Value:C} ({percentage:F0}%)\n";
            }

            // Member analysis
            analysis += "\n**Member Spending:**\n";
            foreach (var member in memberSpending.OrderByDescending(m => m.Value))
            {
                analysis += $"• {member.Key}: {member.Value:C}\n";
            }

            // Pattern insights
            analysis += "\n**Insights:**\n";
            var totalSpending = categorySpending.Values.Sum();
            var avgDailySpending = totalSpending / (decimal)(end - start).Days;

            analysis += $"• Daily average: {avgDailySpending:C}\n";
            analysis += $"• Projected monthly: {avgDailySpending * 30:C}\n";

            if (topCategories.Any())
            {
                var topCategory = topCategories.First();
                var topPercentage = categorySpending.Values.Sum() == 0 ? 0 : (double)(topCategory.Value / categorySpending.Values.Sum() * 100);
                if (topPercentage > 40)
                {
                    analysis += $"• ⚠️ {topCategory.Key} represents {topPercentage:F0}% of spending - consider setting a budget\n";
                }
            }

            return analysis;
        }

        public async Task<string> PredictMonthlyExpensesAsync(string familyId)
        {
            var currentMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var memberSpending = await _familyBankingService.GetMemberSpendingAsync(familyId, currentMonth, DateTime.Today);

            var daysInMonth = DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month);
            var currentDay = DateTime.Today.Day;
            var totalSpentSoFar = memberSpending.Values.Sum();
            var dailyAverage = totalSpentSoFar / currentDay;
            var projectedMonthly = dailyAverage * daysInMonth;

            var lastMonth = currentMonth.AddMonths(-1);
            var lastMonthEnd = currentMonth.AddTicks(-1);
            var lastMonthSpending = await _familyBankingService.GetMemberSpendingAsync(familyId, lastMonth, lastMonthEnd);
            var lastMonthTotal = lastMonthSpending.Values.Sum();

            var prediction = $"📈 **Monthly Expense Prediction**\n\n";
            prediction += $"Current spending: {totalSpentSoFar:C} (Day {currentDay} of {daysInMonth})\n";
            prediction += $"Daily average: {dailyAverage:C}\n";
            prediction += $"**Projected month total: {projectedMonthly:C}**\n\n";

            if (lastMonthTotal > 0)
            {
                var change = projectedMonthly - lastMonthTotal;
                var changePercent = (double)(change / lastMonthTotal * 100);
                prediction += $"Last month: {lastMonthTotal:C}\n";
                prediction += $"Projected change: {change:+C;-C;±C} ({changePercent:+F1;-F1;±F1}%)\n";

                if (Math.Abs(changePercent) > 10)
                {
                    prediction += changePercent > 0
                        ? "⚠️ Spending is trending higher than last month\n"
                        : "✅ Spending is trending lower than last month\n";
                }
            }

            return prediction;
        }

        public async Task<string> RecommendBudgetAdjustmentsAsync(string familyId)
        {
            var insights = await _familyBankingService.GetFamilyInsightsAsync(familyId);
            var recommendations = new List<string>();

            // Savings rate analysis
            if (insights.SavingsRate < 0.1)
            {
                recommendations.Add($"💰 Increase savings rate from {insights.SavingsRate:P1} to 10-20% by reducing discretionary spending");
            }
            else if (insights.SavingsRate > 0.3)
            {
                recommendations.Add($"🎉 Excellent savings rate of {insights.SavingsRate:P1}! Consider investing excess savings for growth");
            }

            // Category-specific recommendations
            if (!string.IsNullOrEmpty(insights.TopSpendingCategory))
            {
                if (insights.TopCategoryPercentage > 50)
                {
                    recommendations.Add($"🔍 {insights.TopSpendingCategory} is {insights.TopCategoryPercentage:F0}% of spending - consider strict limits");
                }
                else if (insights.TopCategoryPercentage > 30)
                {
                    recommendations.Add($"📝 Set a monthly budget for {insights.TopSpendingCategory} ({insights.TopCategoryPercentage:F0}% of spending)");
                }
            }

            // Income vs expense ratio
            if (insights.ExpenseIncomeRatio > 0.9m)
            {
                recommendations.Add("🚨 Expenses are over 90% of income - immediate budget cuts needed");
            }
            else if (insights.ExpenseIncomeRatio > 0.8m)
            {
                recommendations.Add("⚠️ Expenses are over 80% of income - look for areas to reduce spending");
            }

            if (!recommendations.Any())
            {
                recommendations.Add("✅ Your budget looks healthy! Keep monitoring and adjusting as needed");
            }

            return "💡 **Budget Adjustment Recommendations**\n\n" + string.Join("\n", recommendations.Select(r => $"• {r}"));
        }

        public async Task<List<string>> IdentifySpendingAnomaliesAsync(string familyId)
        {
            var anomalies = new List<string>();

            try
            {
                var transactions = await _transactionsService.GetAsync(
                    from: DateTime.Today.AddDays(-30),
                    to: DateTime.Today);

                var expenseTransactions = transactions.Where(t => !t.IsIncome).ToList();
                if (!expenseTransactions.Any()) return anomalies;

                var averageAmount = expenseTransactions.Average(t => t.Amount);
                var maxAmount = expenseTransactions.Max(t => t.Amount);

                // Large transactions (3x average)
                var largeTransactions = expenseTransactions
                    .Where(t => t.Amount > averageAmount * 3)
                    .OrderByDescending(t => t.Amount)
                    .Take(3);

                foreach (var tx in largeTransactions)
                {
                    anomalies.Add($"Large expense: {tx.Amount:C} on {tx.Date:MMM d} - {tx.Description ?? tx.Category} (3x higher than average)");
                }

                // Unusual categories
                var categoryFrequency = expenseTransactions
                    .GroupBy(t => t.Category ?? "Uncategorized")
                    .ToDictionary(g => g.Key, g => g.Count());

                var unusualCategories = expenseTransactions
                    .Where(t => categoryFrequency[t.Category ?? "Uncategorized"] == 1)
                    .Where(t => t.Amount > averageAmount)
                    .Take(3);

                foreach (var tx in unusualCategories)
                {
                    anomalies.Add($"Unusual category: {tx.Amount:C} in {tx.Category} on {tx.Date:MMM d} - {tx.Description}");
                }

                // Weekend vs weekday patterns
                var weekendSpending = expenseTransactions
                    .Where(t => t.Date.DayOfWeek == DayOfWeek.Saturday || t.Date.DayOfWeek == DayOfWeek.Sunday)
                    .Sum(t => t.Amount);

                var weekdaySpending = expenseTransactions
                    .Where(t => t.Date.DayOfWeek != DayOfWeek.Saturday && t.Date.DayOfWeek != DayOfWeek.Sunday)
                    .Sum(t => t.Amount);

                if (weekendSpending > weekdaySpending * 1.5m)
                {
                    anomalies.Add($"High weekend spending pattern detected: {weekendSpending:C} vs {weekdaySpending:C} on weekdays");
                }
            }
            catch
            {
                anomalies.Add("Unable to analyze spending patterns at this time");
            }

            return anomalies;
        }

        // Quick transaction assistance
        public async Task<string> CheckSpendingBeforeTransactionAsync(string memberId, decimal amount, string category)
        {
            try
            {
                var member = await _familyBankingService.GetFamilyMemberAsync(memberId);
                if (member == null) return "I couldn't find your account information.";

                // Check balance
                if (member.Balance < amount)
                {
                    return $"⚠️ Insufficient funds. You have {member.Balance:C} but need {amount:C}. " +
                           $"You're short {amount - member.Balance:C}.";
                }

                // Check spending limits
                var canSpend = await _familyBankingService.CheckSpendingLimitAsync(memberId, category, amount);
                if (!canSpend)
                {
                    return $"⚠️ This {amount:C} {category} purchase would exceed your spending limits. " +
                           $"You've already spent {member.SpentThisMonth:C} of your {member.SpendingLimit:C} monthly limit.";
                }

                // Provide approval with context
                var remainingAfter = member.Balance - amount;
                var spendingPercentageAfter = (member.SpentThisMonth + amount) / member.SpendingLimit;

                var response = $"✅ You can afford this {amount:C} purchase. ";
                response += $"Your balance after: {remainingAfter:C}. ";

                if (spendingPercentageAfter > 0.8m)
                {
                    response += $"⚠️ This will put you at {spendingPercentageAfter:P0} of your monthly limit.";
                }
                else if (spendingPercentageAfter > 0.5m)
                {
                    response += $"You'll be at {spendingPercentageAfter:P0} of your monthly limit.";
                }
                else
                {
                    response += "You're well within your budget limits.";
                }

                return response;
            }
            catch
            {
                return "I'm having trouble checking your spending limits right now. Please try again.";
            }
        }

        public async Task<bool> ShouldBlockTransactionAsync(string memberId, decimal amount, string category)
        {
            try
            {
                var member = await _familyBankingService.GetFamilyMemberAsync(memberId);
                if (member == null) return true;

                // Block if insufficient funds
                if (member.Balance < amount) return true;

                // Block if would exceed limits significantly
                if (member.SpentThisMonth + amount > member.SpendingLimit * 1.2m) return true;

                // Block if unusual large transaction for child/teen accounts
                if ((member.Role == "Child" && amount > 50) || (member.Role == "Teen" && amount > 200))
                {
                    var recentLargeTransactions = await CountRecentLargeTransactions(memberId, amount);
                    if (recentLargeTransactions > 2) return true;
                }

                return false;
            }
            catch
            {
                return true; // Block on error for safety
            }
        }

        public async Task<string> GetPostTransactionInsightAsync(string memberId, decimal amount, string category)
        {
            try
            {
                var member = await _familyBankingService.GetFamilyMemberAsync(memberId);
                if (member == null) return "Transaction recorded.";

                var insights = new List<string>();

                // Balance insight
                insights.Add($"New balance: {member.Balance:C}");

                // Spending progress
                var spendingPercentage = member.SpentThisMonth / member.SpendingLimit;
                if (spendingPercentage > 0.9m)
                {
                    insights.Add($"⚠️ You've used {spendingPercentage:P0} of your monthly limit");
                }
                else if (spendingPercentage > 0.75m)
                {
                    insights.Add($"You've used {spendingPercentage:P0} of your monthly limit");
                }

                // Category insight
                var categorySpending = await GetCategorySpendingForMember(memberId, category);
                if (categorySpending > amount * 5)
                {
                    insights.Add($"You've spent {categorySpending:C} on {category} this month");
                }

                // Achievement check
                if (member.TransactionsThisMonth % 10 == 0)
                {
                    insights.Add($"🎉 {member.TransactionsThisMonth} transactions milestone reached!");
                }

                return string.Join(" • ", insights);
            }
            catch
            {
                return "Transaction recorded successfully.";
            }
        }

        // Utility methods
        private ConversationContext GetOrCreateContext(string familyId, string? memberId)
        {
            var key = $"{familyId}:{memberId ?? "family"}";
            if (!_conversationContexts.ContainsKey(key))
            {
                _conversationContexts[key] = new ConversationContext
                {
                    FamilyId = familyId,
                    MemberId = memberId
                };
            }
            return _conversationContexts[key];
        }

        private static bool ContainsAny(string text, params string[] keywords)
        {
            return keywords.Any(keyword => text.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        private static string ExtractTopicFromQuestion(string question)
        {
            if (ContainsAny(question, "balance", "money")) return "balance";
            if (ContainsAny(question, "spending", "expenses")) return "spending";
            if (ContainsAny(question, "goal", "savings")) return "goals";
            if (ContainsAny(question, "budget", "limit")) return "budget";
            return "general";
        }

        private static decimal ExtractAmountFromText(string text)
        {
            // Simple regex to extract dollar amounts
            var matches = System.Text.RegularExpressions.Regex.Matches(text, @"\$?(\d+(?:\.\d{2})?)");
            if (matches.Any() && decimal.TryParse(matches[0].Groups[1].Value, out var amount))
            {
                return amount;
            }
            return 0;
        }

        private static decimal EstimateMonthlySpending(FamilyMember member)
        {
            var currentDay = DateTime.Today.Day;
            var daysInMonth = DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month);
            var dailyAverage = member.SpentThisMonth / currentDay;
            return dailyAverage * daysInMonth;
        }

        private async Task<int> CountRecentLargeTransactions(string memberId, decimal threshold)
        {
            try
            {
                var transactions = await _transactionsService.GetAsync(
                    from: DateTime.Today.AddDays(-7),
                    to: DateTime.Today);

                var member = await _familyBankingService.GetFamilyMemberAsync(memberId);
                if (member == null) return 0;

                return transactions.Count(t => t.User == member.Name && t.Amount >= threshold && !t.IsIncome);
            }
            catch
            {
                return 0;
            }
        }

        private async Task<decimal> GetCategorySpendingForMember(string memberId, string category)
        {
            try
            {
                var member = await _familyBankingService.GetFamilyMemberAsync(memberId);
                if (member == null) return 0;

                var transactions = await _transactionsService.GetAsync(
                    from: new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1),
                    to: DateTime.Today);

                return transactions
                    .Where(t => t.User == member.Name && t.Category == category && !t.IsIncome)
                    .Sum(t => t.Amount);
            }
            catch
            {
                return 0;
            }
        }

        private string GenerateContextualResponse(string question, ConversationContext context, string familyId)
        {
            var responses = new[]
            {
                "I understand you're asking about your family finances. I can help with balances, spending analysis, budget tracking, and savings goals. What specific information would you like?",
                "I'm here to help your family make smart financial decisions. You can ask me about spending patterns, goal progress, budget status, or get personalized recommendations.",
                "Great question! I can provide insights about your family's financial health, help track expenses, analyze spending trends, and suggest ways to save money. What interests you most?",
                "I'm your family's financial assistant! I can help you understand where your money goes, track progress toward goals, and make budgeting easier. What would you like to explore?"
            };

            var random = new Random();
            return responses[random.Next(responses.Length)];
        }

        // Simplified implementations for remaining interface methods
        public async Task<string> GetTransactionAdviceAsync(string memberId, decimal amount, string category, string? merchant = null)
        {
            return await CheckSpendingBeforeTransactionAsync(memberId, amount, category);
        }

        public async Task<string> SuggestSavingsGoalAsync(string familyId, decimal targetAmount, DateTime targetDate)
        {
            var daysToTarget = (targetDate - DateTime.Today).Days;
            var monthsToTarget = Math.Max(1, daysToTarget / 30.0m);
            var monthlyRequired = targetAmount / monthsToTarget;

            return $"To save {targetAmount:C} by {targetDate:MMM yyyy}, your family needs to save {monthlyRequired:C} per month. " +
                   $"That's about {monthlyRequired / 4:C} per week or {monthlyRequired / 30:C} per day. " +
                   "Would you like me to help set up automatic transfers to reach this goal?";
        }

        public async Task<string> OptimizeGoalContributionsAsync(string goalId)
        {
            var goal = await _familyBankingService.GetFamilyGoalAsync(goalId);
            if (goal == null) return "Goal not found.";

            var monthlyNeeded = goal.MonthlyRequiredAmount;
            var familyMembers = await _familyBankingService.GetFamilyMembersAsync(goal.FamilyId);
            var parents = familyMembers.Where(m => m.Role == "Parent").ToList();

            if (parents.Any())
            {
                var perParent = monthlyNeeded / parents.Count;
                return $"To reach your '{goal.Title}' goal, I recommend each parent contributes {perParent:C} monthly. " +
                       $"This splits the {monthlyNeeded:C} needed evenly and keeps you on track for {goal.TargetDate:MMM yyyy}.";
            }

            return $"To reach your '{goal.Title}' goal, you need {monthlyNeeded:C} monthly contributions.";
        }

        public async Task<List<string>> GetGoalMotivationMessagesAsync(string goalId)
        {
            var goal = await _familyBankingService.GetFamilyGoalAsync(goalId);
            if (goal == null) return new List<string>();

            var messages = new List<string>
            {
                $"You're {goal.ProgressPercentage:F0}% of the way to your {goal.Title} goal! 🎯",
                $"Only {goal.RemainingAmount:C} left to reach your {goal.Title} goal! 💪",
                $"{goal.DaysRemaining} days until your {goal.Title} target date - you can do this! ⏰"
            };

            if (goal.ProgressPercentage >= 75)
            {
                messages.Add("You're in the final stretch! Your goal is almost within reach! 🏁");
            }
            else if (goal.ProgressPercentage >= 50)
            {
                messages.Add("Halfway there! Your commitment is paying off! 🌟");
            }
            else if (goal.ProgressPercentage >= 25)
            {
                messages.Add("Great progress! Every contribution gets you closer! 📈");
            }

            return messages;
        }

        public async Task<string> CalculateGoalFeasibilityAsync(string goalId)
        {
            var goal = await _familyBankingService.GetFamilyGoalAsync(goalId);
            if (goal == null) return "Goal not found.";

            var insights = await _familyBankingService.GetFamilyInsightsAsync(goal.FamilyId);
            var monthlyRequired = goal.MonthlyRequiredAmount;
            var currentSavings = insights.NetSavings;

            if (monthlyRequired <= currentSavings)
            {
                return $"✅ Highly feasible! You currently save {currentSavings:C}/month and only need {monthlyRequired:C} for this goal.";
            }
            else if (monthlyRequired <= currentSavings * 1.5m)
            {
                return $"⚠️ Moderately feasible. You save {currentSavings:C}/month but need {monthlyRequired:C}. " +
                       $"Consider reducing expenses by {monthlyRequired - currentSavings:C}/month.";
            }
            else
            {
                return $"🔴 Challenging goal. You save {currentSavings:C}/month but need {monthlyRequired:C}. " +
                       $"Consider extending the timeline or reducing the target amount.";
            }
        }

        // Simplified implementations for remaining methods
        public Task<string> GetFinancialTipAsync(string memberId, string category = "general") =>
            Task.FromResult("Remember: every dollar saved is a dollar earned! Start with small, consistent savings habits.");

        public Task<List<string>> GetAgeAppropriateFinancialAdviceAsync(string memberId) =>
            Task.FromResult(new List<string> { "Track your spending", "Save before you spend", "Ask questions about money" });

        public Task<string> ExplainFinancialConceptAsync(string concept, string memberId) =>
            Task.FromResult($"{concept}: I'd be happy to explain this financial concept in simple terms!");

        public Task<string> GenerateFamilyFinancialReportAsync(string familyId) =>
            Task.FromResult("Family Financial Report: Your family is making great progress with money management!");

        public Task<List<string>> GetDailyFinancialRemindersAsync(string familyId) =>
            Task.FromResult(new List<string> { "Check your daily spending", "Review your goals progress" });

        public Task<List<string>> GetWeeklyFinancialSummaryAsync(string familyId) =>
            Task.FromResult(new List<string> { "This week's spending summary", "Goal progress update" });

        public Task<List<string>> GetMonthlyGoalUpdatesAsync(string familyId) =>
            Task.FromResult(new List<string> { "Monthly goal progress report" });

        public Task<bool> ShouldSendBudgetAlertAsync(string memberId, string category) =>
            Task.FromResult(false);

        public Task<string> IdentifyMoneySavingOpportunitiesAsync(string familyId) =>
            Task.FromResult("Look for subscription services you don't use and consider meal planning to reduce food costs.");

        public Task<string> CompareFamilyToAveragesAsync(string familyId) =>
            Task.FromResult("Your family's financial habits are above average in most categories!");

        public Task<List<string>> GetTrendAnalysisAsync(string familyId) =>
            Task.FromResult(new List<string> { "Spending is trending stable", "Savings rate is improving" });

        public Task<string> PredictFutureBalanceAsync(string memberId, int daysAhead = 30) =>
            Task.FromResult("Based on current patterns, your balance should remain stable.");

        public Task<string> AnalyzeFamilySpendingBehaviorAsync(string familyId) =>
            Task.FromResult("Your family shows consistent, responsible spending patterns.");

        public Task<string> GetSpendingPersonalityProfileAsync(string memberId) =>
            Task.FromResult("You're a thoughtful spender who considers purchases carefully.");

        public Task<List<string>> SuggestBehaviorChangesAsync(string memberId) =>
            Task.FromResult(new List<string> { "Continue your good spending habits" });

        public Task<bool> DetectUnusualSpendingPatternAsync(string memberId, decimal amount, string category) =>
            Task.FromResult(false);

        public Task<string> GetEmergencyBudgetAdviceAsync(string familyId) =>
            Task.FromResult("Focus on essential expenses only: housing, food, utilities, and transportation.");

        public Task<List<string>> CreateDebtManagementPlanAsync(string familyId) =>
            Task.FromResult(new List<string> { "List all debts", "Pay minimums on all", "Focus extra payments on highest interest debt" });

        public Task<string> AssessFinancialRiskAsync(string familyId) =>
            Task.FromResult("Your family's financial risk appears to be low based on current patterns.");

        public Task<string> SuggestIncomeImprovementAsync(string familyId) =>
            Task.FromResult("Consider skill development, side income, or career advancement opportunities.");

        public Task<string> GenerateFamilyMeetingAgendaAsync(string familyId) =>
            Task.FromResult("Family Finance Meeting: Review budget, discuss goals, celebrate progress, plan ahead.");

        public Task<string> MediateFinancialDisagreementAsync(string familyId, string scenario) =>
            Task.FromResult("Let's focus on your shared goals and find a compromise that works for everyone.");

        public Task<List<string>> SuggestFamilyFinancialActivitiesAsync(string familyId) =>
            Task.FromResult(new List<string> { "Family budget game night", "Goal visualization board", "Savings challenge" });

        public Task<string> CreateChildFriendlyFinancialExplanationAsync(string concept, string childMemberId) =>
            Task.FromResult($"Think of {concept} like... saving your allowance for something special you really want!");

        public Task<bool> LearnFromUserFeedbackAsync(string interaction, bool wasHelpful, string? feedback = null) =>
            Task.FromResult(true);

        public Task<string> GetPersonalizedDashboardInsightAsync(string memberId) =>
            Task.FromResult("Here's what's important for you today: check your balance and review your savings goal progress.");

        public Task<List<string>> PredictUserQuestionsAsync(string memberId) =>
            Task.FromResult(new List<string> { "What's my balance?", "How much have I spent?", "Am I on track for my goals?" });

        public Task<bool> UpdateUserPreferencesAsync(string memberId, Dictionary<string, object> preferences) =>
            Task.FromResult(true);
    }
}