using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace EnhancedLifeSimulator
{
    #region 全局状态与辅助方法
    static class GlobalState
    {
        public static int EconomicIndex = 100;
        public static double ClimateFactor = 1.0;
        public static int GlobalStockMarket = 1000;
        public static int CityDevelopmentIndex = 100;
        public static string Language = "zh";
        public static bool ShowAgeCommentary = true;
        public static bool ShowDetailedEvents = true;
        public static bool AutoSave = false;
        public static ConsoleColor MainColor = ConsoleColor.Cyan;
        public static ConsoleColor HighlightColor = ConsoleColor.Yellow;
        public static ConsoleColor WarningColor = ConsoleColor.Red;
        public static ConsoleColor SuccessColor = ConsoleColor.Green;

        public static string GetMessage(string key)
        {
            var messages = new Dictionary<string, (string zh, string en)>
            {
                { "welcome", ("欢迎来到人生模拟器", "Welcome to Life Simulator") },
                { "newGame", ("新游戏", "New Game") },
                { "loadGame", ("读档", "Load Game") },
                { "chooseSlot", ("请输入存档槽位编号 (1-3):", "Enter save slot number (1-3):") },
                { "gameSaved", ("游戏已保存到", "Game saved to") },
                { "gameLoaded", ("游戏已从", "Game loaded from") },
                { "loadFailed", ("读档失败，创建新游戏。", "Load failed, creating new game.") },
                { "inputError", ("输入错误，创建新游戏。", "Input error, creating new game.") },
                { "year", ("年龄", "Age") },
                { "lifeSummary", ("人生总结", "Life Summary") },
                { "thanksPlay", ("感谢游玩！", "Thanks for playing!") },
                { "pressAnyKey", ("按任意键退出...", "Press any key to exit...") },
                { "settings", ("设置", "Settings") },
                { "showCommentary", ("显示年龄解说", "Show age commentary") },
                { "showEvents", ("显示详细事件", "Show detailed events") },
                { "autoSave", ("自动保存", "Auto save") },
                { "back", ("返回", "Back") },
                { "colorScheme", ("颜色方案", "Color scheme") },
                { "nextYear", ("进入下一年", "Next year") },
                { "saveGame", ("保存游戏", "Save game") },
                { "quit", ("退出", "Quit") },
                { "menu", ("菜单", "Menu") }
            };

            return messages.ContainsKey(key)
                ? (Language == "zh" ? messages[key].zh : messages[key].en)
                : key;
        }

        public static void PrintDivider(string title = "")
        {
            Console.ForegroundColor = MainColor;
            if (!string.IsNullOrEmpty(title))
                Console.WriteLine($"╠══ {title} {new string('═', Math.Max(0, 50 - title.Length))}╣");
            else
                Console.WriteLine($"╔{new string('═', 58)}╗");
            Console.ResetColor();
        }

        public static void PrintCentered(string text, ConsoleColor color = ConsoleColor.White)
        {
            Console.ForegroundColor = color;
            int padding = (60 - text.Length) / 2;
            if (padding < 0) padding = 0;
            Console.WriteLine($"{new string(' ', padding)}{text}");
            Console.ResetColor();
        }

        public static void PrintWithColor(string text, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ResetColor();
        }
    }
    #endregion

    #region 角色类及跨代遗传
    class Character
    {
        public int Age { get; set; }
        public int Health { get; set; }
        public int Strength { get; set; }
        public int Intelligence { get; set; }
        public int Charisma { get; set; }
        public int Wealth { get; set; }
        public int Happiness { get; set; }
        public Career CurrentCareer { get; set; }
        public int Generation { get; set; }

        public int Programming { get; set; }
        public int Art { get; set; }
        public int Sports { get; set; }
        public int Leadership { get; set; }
        public int InvestmentSkill { get; set; }

        // 原始天赋点记录
        public int OriginalStrength { get; set; }
        public int OriginalIntelligence { get; set; }
        public int OriginalCharisma { get; set; }
        public int OriginalProgramming { get; set; }
        public int OriginalArt { get; set; }
        public int OriginalSports { get; set; }
        public int OriginalLeadership { get; set; }
        public int OriginalInvestmentSkill { get; set; }

        public List<string> Achievements { get; set; }
        public Dictionary<string, int> Relationships { get; set; }
        public bool HasHereditaryDisease { get; set; }
        public int ChronicDiseaseLevel { get; set; }
        public int MajorDiseaseCount { get; set; }
        public List<string> LifeEvents { get; set; }
        public List<string> Milestones { get; set; }

        public Character(int strength, int intelligence, int charisma,
                         int programming, int art, int sports, int leadership, int investmentSkill, Character parent = null)
        {
            Age = 0;
            if (parent != null)
            {
                Generation = parent.Generation + 1;
                Strength = strength + new Random().Next(-2, 3);
                Intelligence = intelligence + new Random().Next(-2, 3);
                Charisma = charisma + new Random().Next(-2, 3);
                Programming = Math.Max(0, programming + new Random().Next(-1, 2));
                Art = Math.Max(0, art + new Random().Next(-1, 2));
                Sports = Math.Max(0, sports + new Random().Next(-1, 2));
                Leadership = Math.Max(0, leadership + new Random().Next(-1, 2));
                InvestmentSkill = Math.Max(0, investmentSkill + new Random().Next(-1, 2));

                OriginalStrength = Strength;
                OriginalIntelligence = Intelligence;
                OriginalCharisma = Charisma;
                OriginalProgramming = Programming;
                OriginalArt = Art;
                OriginalSports = Sports;
                OriginalLeadership = Leadership;
                OriginalInvestmentSkill = InvestmentSkill;

                Health = 100;
                Happiness = 50;
                Wealth = parent.Wealth / 4; // 继承部分财富
                CurrentCareer = Career.Unemployed;
                HasHereditaryDisease = (parent.HasHereditaryDisease || new Random().NextDouble() < 0.2);
            }
            else
            {
                Generation = 1;
                Strength = strength;
                Intelligence = intelligence;
                Charisma = charisma;
                Programming = programming;
                Art = art;
                Sports = sports;
                Leadership = leadership;
                InvestmentSkill = investmentSkill;

                OriginalStrength = strength;
                OriginalIntelligence = intelligence;
                OriginalCharisma = charisma;
                OriginalProgramming = programming;
                OriginalArt = art;
                OriginalSports = sports;
                OriginalLeadership = leadership;
                OriginalInvestmentSkill = investmentSkill;

                HasHereditaryDisease = (new Random().NextDouble() < 0.3);
                Health = 100;
                Happiness = 50;
                Wealth = 0;
                CurrentCareer = Career.Unemployed;
            }
            Achievements = new List<string>();
            Relationships = new Dictionary<string, int>();
            ChronicDiseaseLevel = 0;
            MajorDiseaseCount = 0;
            LifeEvents = new List<string>();
            Milestones = new List<string>();
        }

        public bool IsAlive() => Health > 0 && Age < 120;

        public void AgeOneYear()
        {
            Age++;
            int decline = Age / 10;
            if (Age >= 65) decline += 2;
            Health -= decline;
            if (Health < 0) Health = 0;
        }

        public void ShowStats()
        {
            GlobalState.PrintDivider("当前角色状态");
            Console.WriteLine($"║ 第 {Generation} 代");
            Console.WriteLine($"║ {GlobalState.GetMessage("year")}: {Age}");

            PrintStatBar("健康", Health, 100, ConsoleColor.Red);
            PrintStatBar("力量", Strength, 20, ConsoleColor.DarkYellow);
            PrintStatBar("智力", Intelligence, 20, ConsoleColor.Blue);
            PrintStatBar("魅力", Charisma, 20, ConsoleColor.Magenta);
            PrintStatBar("财富", Wealth, 50000, ConsoleColor.Green);
            PrintStatBar("幸福", Happiness, 100, ConsoleColor.Yellow);

            Console.WriteLine($"║ 职业: {CurrentCareer}");
            Console.WriteLine($"║ 编程: {Programming} (初始 {OriginalProgramming})");
            Console.WriteLine($"║ 艺术: {Art} (初始 {OriginalArt})");
            Console.WriteLine($"║ 运动: {Sports} (初始 {OriginalSports})");
            Console.WriteLine($"║ 领导力: {Leadership} (初始 {OriginalLeadership})");
            Console.WriteLine($"║ 投资技能: {InvestmentSkill} (初始 {OriginalInvestmentSkill})");
            Console.WriteLine($"║ 慢性病级别: {ChronicDiseaseLevel}  重大疾病数: {MajorDiseaseCount}");
            Console.WriteLine($"║ 成就: {(Achievements.Count == 0 ? "无" : string.Join(", ", Achievements))}");
            Console.WriteLine($"║ 人际关系: {(Relationships.Count == 0 ? "无" : string.Join(", ", Relationships))}");
            GlobalState.PrintDivider();
        }

        private void PrintStatBar(string name, int value, int max, ConsoleColor color)
        {
            int barLength = 20;
            int filled = (int)Math.Round((double)value / max * barLength);
            filled = Math.Min(filled, barLength);

            Console.Write($"║ {name}: ");
            Console.ForegroundColor = color;
            Console.Write($"{value} [{new string('█', filled)}{new string('░', barLength - filled)}]");
            Console.ResetColor();
            Console.WriteLine();
        }
    }

    enum Career
    {
        Unemployed,
        Student,
        Intern,
        Employee,
        SeniorEmployee,
        Manager,
        SeniorManager,
        Director,
        VP,
        CTO,
        CEO,
        Entrepreneur,
        Retired
    }
    #endregion

    #region 事件系统
    static class EventManager
    {
        static Random rand = new Random();
        private static List<string> firstNames = new List<string> { "张", "王", "李", "赵", "刘", "陈", "杨", "黄", "周", "吴" };
        private static List<string> lastNames = new List<string> { "伟", "芳", "娜", "秀英", "敏", "静", "丽", "强", "磊", "洋" };

        public static void TriggerRandomEvent(Character character)
        {
            try
            {
                int eventType = rand.Next(1, 101);
                string eventDescription = "";
                string friendName = $"{firstNames[rand.Next(firstNames.Count)]}{lastNames[rand.Next(lastNames.Count)]}";

                if (eventType <= 5) // 5% 好运事件
                {
                    int gain = rand.Next(100, 1000);
                    character.Wealth += gain;
                    character.Happiness += 5;
                    character.Health = Math.Min(character.Health + 2, 100);
                    character.Achievements.Add($"好运 {character.Age}岁");
                    eventDescription = $"🎉【好运事件】你中了彩票，获得{gain}元奖金！";
                }
                else if (eventType <= 10) // 5% 意外事故
                {
                    int loss = rand.Next(5, 20);
                    character.Health -= loss;
                    character.Happiness -= 5;
                    if (rand.NextDouble() < 0.3)
                    {
                        character.MajorDiseaseCount++;
                        eventDescription = $"⚠️【重大疾病】你患上了严重疾病！健康损失{loss}点";
                    }
                    else
                    {
                        eventDescription = $"⚠️【意外事故】你遭遇了小事故，健康损失{loss}点";
                    }
                }
                else if (eventType <= 15) // 5% 职业发展
                {
                    int chance = rand.Next(0, 100);
                    if (character.Intelligence + character.Leadership > chance)
                    {
                        character.CurrentCareer = (Career)Math.Min((int)character.CurrentCareer + 1, (int)Career.Entrepreneur);
                        eventDescription = $"💼【职业晋升】你被提升为: {character.CurrentCareer}";
                        character.Achievements.Add($"升职 {character.CurrentCareer} ({character.Age}岁)");
                        character.Milestones.Add($"在{character.Age}岁晋升为{character.CurrentCareer}");
                    }
                    else
                    {
                        eventDescription = "💼【职业发展】一个职业机会擦肩而过。";
                    }
                }
                else if (eventType <= 20) // 5% 新朋友
                {
                    eventDescription = $"👫【新朋友】你认识了新朋友: {friendName}";
                    if (!character.Relationships.ContainsKey(friendName))
                        character.Relationships.Add(friendName, rand.Next(1, 11));
                    else
                        character.Relationships[friendName] += rand.Next(1, 5);
                    character.Happiness += 3;
                }
                // ... (其他事件类似，限于篇幅省略部分事件代码)
                else if (eventType <= 25) // 5% 技能提升
                {
                    int skillChoice = rand.Next(1, 6);
                    switch (skillChoice)
                    {
                        case 1:
                            character.Programming += 1;
                            eventDescription = "📚【技能提升】通过自学，你的编程技能提升了！";
                            break;
                        case 2:
                            character.Art += 1;
                            eventDescription = "🎨【技能提升】参加艺术课程，你的艺术技能提升了！";
                            break;
                        case 3:
                            character.Sports += 1;
                            eventDescription = "🏃【技能提升】坚持锻炼，你的运动技能提升了！";
                            break;
                        case 4:
                            character.Leadership += 1;
                            eventDescription = "👔【技能提升】领导项目，你的领导力提升了！";
                            break;
                        case 5:
                            character.InvestmentSkill += 1;
                            eventDescription = "📈【技能提升】研究市场，你的投资技能提升了！";
                            break;
                    }
                }
                // ... (其他事件类似)

                // 记录事件
                character.LifeEvents.Add($"{character.Age}岁: {eventDescription}");
                if (eventType <= 15 || eventType >= 90) // 重要事件
                {
                    character.Milestones.Add(eventDescription);
                }

                // 显示事件
                if (GlobalState.ShowDetailedEvents)
                {
                    Console.ForegroundColor = eventDescription.Contains("⚠️") ? GlobalState.WarningColor :
                                           eventDescription.Contains("🎉") ? GlobalState.SuccessColor : GlobalState.MainColor;
                    Console.WriteLine(eventDescription);
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("事件执行时出错：" + ex.Message);
            }
        }
    }
    #endregion

    #region 年龄段解说系统
    static class AgeCommentary
    {
        private static Dictionary<int, string> ageComments = new Dictionary<int, string>()
        {
            {0, "👶 出生：你来到了这个世界，开始了人生的旅程。"},
            {1, "👶 1岁：你正在学习走路和说话，对世界充满好奇。"},
            {2, "👶 2岁：你开始探索周围的环境，表现出自己的个性。"},
            {3, "👧 3岁：你开始上幼儿园，学习基本的社交技能。"},
            // ... (其他年龄解说)
            {18, "🎓 18岁：你高中毕业，面临人生重要选择 - 大学还是工作？"},
            {25, "💼 25岁：你在职场中逐渐站稳脚跟，开始思考职业发展。"},
            {30, "🏠 30岁：而立之年，你可能已经成家立业，责任重大。"},
            {40, "👨‍💼 40岁：不惑之年，你在事业上可能达到巅峰，家庭稳定。"},
            {60, "👴 60岁：退休生活开始，你有更多时间陪伴家人和发展爱好。"},
            {80, "👵 80岁：耄耋之年，你成为家族的长者和智慧的象征。"},
            {100, "🎂 100岁：百岁寿星！你创造了家族的长寿记录。"}
        };

        public static string GetCommentary(int age)
        {
            if (ageComments.ContainsKey(age))
                return ageComments[age];

            if (age < 10) return $"👦 {age}岁：快乐的童年时光，你在学校和家庭中成长。";
            else if (age < 20) return $"👩‍🎓 {age}岁：青少年时期，你在学业和社交中探索自我。";
            else if (age < 30) return $"👩‍💼 {age}岁：年轻的职场人，建立事业和人际关系。";
            else if (age < 40) return $"👨‍💻 {age}岁：事业上升期，你可能组建了家庭。";
            else if (age < 50) return $"👨‍🔧 {age}岁：中年时期，经验丰富但面临新挑战。";
            else if (age < 60) return $"👨‍🏫 {age}岁：准备退休生活，思考人生意义。";
            else if (age < 70) return $"👨‍🌾 {age}岁：享受退休生活，发展新兴趣。";
            else if (age < 80) return $"👴 {age}岁：成为长者，分享智慧和经验。";
            else if (age < 90) return $"👵 {age}岁：平静的晚年，珍惜每一天。";
            else if (age < 100) return $"🎂 {age}岁：长寿典范，见证历史变迁。";
            else return $"🌟 {age}岁：世纪老人！你的人生是部传奇。";
        }

        public static string GetLifeEvaluation(Character character)
        {
            int score = CalculateLifeScore(character);

            string evaluation = GetEvaluationText(score, character);
            string summary = GetLifeSummary(character);
            string advice = GetLifeAdvice(score);

            return $"{evaluation}\n\n{summary}\n\n{advice}";
        }

        private static int CalculateLifeScore(Character character)
        {
            int score = 0;
            score += character.Wealth / 1000;
            score += character.Happiness;
            score += character.Health;
            score += character.Achievements.Count * 5;
            score += character.Relationships.Count * 2;

            // 职业加成
            if (character.CurrentCareer >= Career.Manager) score += 50;
            if (character.CurrentCareer >= Career.Director) score += 80;
            if (character.CurrentCareer >= Career.CEO) score += 100;

            // 年龄调整
            if (character.Age < 50) score = (int)(score * 0.8);
            else if (character.Age > 80) score = (int)(score * 1.2);

            // 疾病惩罚
            score -= character.ChronicDiseaseLevel * 10;
            score -= character.MajorDiseaseCount * 20;

            // 世代加成
            score += character.Generation * 5;

            return score;
        }

        private static string GetEvaluationText(int score, Character character)
        {
            string title;
            ConsoleColor color;

            if (score < 50)
            {
                title = "🌧️ 遗憾的人生";
                color = GlobalState.WarningColor;
            }
            else if (score < 100)
            {
                title = "🌥️ 普通的人生";
                color = ConsoleColor.Gray;
            }
            else if (score < 200)
            {
                title = "⛅ 充实的人生";
                color = ConsoleColor.Cyan;
            }
            else if (score < 300)
            {
                title = "🌤️ 成功的人生";
                color = ConsoleColor.Blue;
            }
            else if (score < 400)
            {
                title = "☀️ 杰出的人生";
                color = GlobalState.SuccessColor;
            }
            else
            {
                title = "🌟 传奇的人生";
                color = GlobalState.HighlightColor;
            }

            string evalText = $"人生评价: {title} (得分: {score})";
            string genText = character.Generation > 1 ? $"作为第{character.Generation}代传人，" : "";
            string legacyText = $"你留下了{(score > 200 ? "丰富" : "有限")}的人生遗产。";

            Console.ForegroundColor = color;
            GlobalState.PrintCentered(evalText, color);
            Console.ResetColor();

            return $"{genText}{legacyText}";
        }

        private static string GetLifeSummary(Character character)
        {
            return $"📅 你活了{character.Age}年\n" +
                   $"💰 最终财富: {character.Wealth}元\n" +
                   $"😊 幸福度: {character.Happiness}/100\n" +
                   $"🏆 成就数: {character.Achievements.Count}\n" +
                   $"👥 人际关系: {character.Relationships.Count}人\n" +
                   $"💊 健康状况: {(character.Health > 70 ? "良好" : "不佳")}";
        }

        private static string GetLifeAdvice(int score)
        {
            if (score < 50) return "人生建议: 多关注健康与人际关系，财富不是唯一追求";
            if (score < 100) return "人生建议: 平衡工作与生活，培养兴趣爱好";
            if (score < 200) return "人生建议: 继续保持，尝试帮助他人";
            if (score < 300) return "人生建议: 你做得很好，可以考虑传承经验";
            return "人生建议: 你的人生堪称典范，是后代的榜样";
        }
    }
    #endregion

    #region 主程序
    class Program
    {
        static void ShowMainMenu()
        {
            Console.Clear();
            Console.ForegroundColor = GlobalState.MainColor;
            Console.WriteLine("╔════════════════════════════════════════════╗");
            Console.WriteLine("║                                            ║");
            GlobalState.PrintCentered("人生模拟器", GlobalState.HighlightColor);
            Console.WriteLine("║                                            ║");
            Console.WriteLine("╠════════════════════════════════════════════╣");
            Console.WriteLine("║                                            ║");
            Console.WriteLine($"║  1. {GlobalState.GetMessage("newGame")}");
            Console.WriteLine($"║  2. {GlobalState.GetMessage("loadGame")}");
            Console.WriteLine($"║  3. {GlobalState.GetMessage("settings")}");
            Console.WriteLine($"║  4. {GlobalState.GetMessage("quit")}");
            Console.WriteLine("║                                            ║");
            Console.WriteLine("╚════════════════════════════════════════════╝");
            Console.ResetColor();
        }

        static void ShowSettingsMenu()
        {
            while (true)
            {
                Console.Clear();
                GlobalState.PrintDivider(GlobalState.GetMessage("settings"));
                Console.WriteLine($"1. {GlobalState.GetMessage("showCommentary")}: {(GlobalState.ShowAgeCommentary ? "✅开" : "❌关")}");
                Console.WriteLine($"2. {GlobalState.GetMessage("showEvents")}: {(GlobalState.ShowDetailedEvents ? "✅开" : "❌关")}");
                Console.WriteLine($"3. {GlobalState.GetMessage("autoSave")}: {(GlobalState.AutoSave ? "✅开" : "❌关")}");
                Console.WriteLine($"4. {GlobalState.GetMessage("colorScheme")}");
                Console.WriteLine($"5. {GlobalState.GetMessage("back")}");
                Console.Write("请选择: ");

                string choice = Console.ReadLine();
                switch (choice)
                {
                    case "1":
                        GlobalState.ShowAgeCommentary = !GlobalState.ShowAgeCommentary;
                        break;
                    case "2":
                        GlobalState.ShowDetailedEvents = !GlobalState.ShowDetailedEvents;
                        break;
                    case "3":
                        GlobalState.AutoSave = !GlobalState.AutoSave;
                        break;
                    case "4":
                        ChangeColorScheme();
                        break;
                    case "5":
                        return;
                    default:
                        Console.WriteLine("无效选择");
                        Thread.Sleep(500);
                        break;
                }
            }
        }

        static void ChangeColorScheme()
        {
            Console.Clear();
            GlobalState.PrintDivider("颜色方案");
            Console.WriteLine("1. 默认 (蓝/黄)");
            Console.WriteLine("2. 自然 (绿/棕)");
            Console.WriteLine("3. 活力 (红/橙)");
            Console.WriteLine("4. 优雅 (紫/粉)");
            Console.WriteLine("5. 返回");
            Console.Write("请选择: ");

            string choice = Console.ReadLine();
            switch (choice)
            {
                case "1":
                    GlobalState.MainColor = ConsoleColor.Cyan;
                    GlobalState.HighlightColor = ConsoleColor.Yellow;
                    break;
                case "2":
                    GlobalState.MainColor = ConsoleColor.Green;
                    GlobalState.HighlightColor = ConsoleColor.DarkYellow;
                    break;
                case "3":
                    GlobalState.MainColor = ConsoleColor.Red;
                    GlobalState.HighlightColor = ConsoleColor.DarkYellow;
                    break;
                case "4":
                    GlobalState.MainColor = ConsoleColor.DarkMagenta;
                    GlobalState.HighlightColor = ConsoleColor.Magenta;
                    break;
                case "5":
                    return;
                default:
                    Console.WriteLine("无效选择");
                    Thread.Sleep(500);
                    break;
            }
        }

        static void ShowYearlyMenu()
        {
            Console.WriteLine($"1. {GlobalState.GetMessage("nextYear")}");
            Console.WriteLine($"2. {GlobalState.GetMessage("saveGame")}");
            Console.WriteLine($"3. {GlobalState.GetMessage("menu")}");
            Console.WriteLine($"4. {GlobalState.GetMessage("quit")}");
            Console.Write("请选择: ");
        }

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.Title = "人生模拟器";

            Character character = null;
            bool running = true;

            while (running)
            {
                ShowMainMenu();
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1": // 新游戏
                        character = CreateNewCharacter();
                        RunLifeSimulation(character);
                        break;
                    case "2": // 读档
                        character = LoadCharacter();
                        if (character != null)
                            RunLifeSimulation(character);
                        break;
                    case "3": // 设置
                        ShowSettingsMenu();
                        break;
                    case "4": // 退出
                        running = false;
                        break;
                    default:
                        Console.WriteLine("无效选择");
                        Thread.Sleep(500);
                        break;
                }
            }

            Console.Clear();
            GlobalState.PrintCentered("感谢游玩人生模拟器！", GlobalState.HighlightColor);
            GlobalState.PrintCentered("再见！", GlobalState.MainColor);
            Thread.Sleep(1000);
        }

        static Character CreateNewCharacter()
        {
            Console.Clear();
            GlobalState.PrintDivider("创建新角色");

            int s, i, c, p, a, sp, l, inv;
            AllocateTalentPoints(out s, out i, out c, out p, out a, out sp, out l, out inv);

            Character character = new Character(s, i, c, p, a, sp, l, inv);
            character.LifeEvents.Add("0岁: 角色创建，开始人生旅程");
            character.Milestones.Add("0岁: 出生");

            return character;
        }

        static Character LoadCharacter()
        {
            Console.Clear();
            GlobalState.PrintDivider(GlobalState.GetMessage("loadGame"));
            Console.WriteLine(GlobalState.GetMessage("chooseSlot"));

            int slot;
            if (int.TryParse(Console.ReadLine(), out slot))
            {
                Character character = SaveLoadManager.LoadGame(slot);
                if (character != null)
                {
                    Console.WriteLine("角色加载成功！");
                    Thread.Sleep(1000);
                    return character;
                }
            }

            Console.WriteLine(GlobalState.GetMessage("loadFailed"));
            Thread.Sleep(1000);
            return null;
        }

        static void RunLifeSimulation(Character character)
        {
            while (character.IsAlive())
            {
                Console.Clear();
                DisplayYearlyStatus(character);

                ShowYearlyMenu();
                string input = Console.ReadLine();

                switch (input)
                {
                    case "1": // 进入下一年
                        ProcessYear(character);
                        break;
                    case "2": // 保存游戏
                        SaveGame(character);
                        break;
                    case "3": // 返回菜单
                        return;
                    case "4": // 退出游戏
                        Environment.Exit(0);
                        break;
                    default:
                        Console.WriteLine("无效输入");
                        Thread.Sleep(500);
                        break;
                }
            }

            // 角色死亡后的处理
            DisplayLifeSummary(character);
        }

        static void DisplayYearlyStatus(Character character)
        {
            GlobalState.PrintDivider($"第 {character.Generation} 代");
            Console.WriteLine($"║ 年龄: {character.Age}岁");

            // 显示年龄解说
            if (GlobalState.ShowAgeCommentary)
            {
                Console.ForegroundColor = GlobalState.HighlightColor;
                Console.WriteLine($"║ {AgeCommentary.GetCommentary(character.Age)}");
                Console.ResetColor();
            }

            character.ShowStats();

            // 显示ASCII状态图
            Visualization.ShowAsciiChart(character);
        }

        static void ProcessYear(Character character)
        {
            character.AgeOneYear();

            // 更新全局状态
            GlobalState.EconomicIndex += new Random().Next(-3, 4);
            GlobalState.ClimateFactor += new Random().NextDouble() * 0.05;
            GlobalState.GlobalStockMarket += new Random().Next(-10, 11);
            GlobalState.CityDevelopmentIndex += new Random().Next(-5, 6);

            // 触发随机事件
            EventManager.TriggerRandomEvent(character);

            // 健康检查
            if (character.Health < 0) character.Health = 0;

            // 退休检查
            if (character.Age >= 65 && character.CurrentCareer != Career.Retired)
            {
                character.CurrentCareer = Career.Retired;
                character.LifeEvents.Add($"{character.Age}岁: 退休");
                character.Milestones.Add($"在{character.Age}岁退休");
            }

            // 自动保存
            if (GlobalState.AutoSave && character.Age % 5 == 0)
            {
                SaveLoadManager.SaveGame(character, 1);
            }

            // 加载动画
            Console.Write("加载中");
            for (int i = 0; i < 3; i++)
            {
                Console.Write(".");
                Thread.Sleep(300);
            }
        }

        static void DisplayLifeSummary(Character character)
        {
            Console.Clear();
            GlobalState.PrintDivider(GlobalState.GetMessage("lifeSummary"));

            // 显示人生评价
            Console.WriteLine(AgeCommentary.GetLifeEvaluation(character));

            // 显示重要里程碑
            GlobalState.PrintDivider("人生里程碑");
            foreach (var milestone in character.Milestones)
            {
                Console.WriteLine($"★ {milestone}");
            }

            Console.WriteLine("\n" + GlobalState.GetMessage("pressAnyKey"));
            Console.ReadKey();
        }

        static void SaveGame(Character character)
        {
            Console.WriteLine(GlobalState.GetMessage("chooseSlot"));
            int slot;
            if (int.TryParse(Console.ReadLine(), out slot))
            {
                SaveLoadManager.SaveGame(character, slot);
                Console.WriteLine("游戏保存成功！");
                Thread.Sleep(1000);
            }
            else
            {
                Console.WriteLine("输入无效");
                Thread.Sleep(500);
            }
        }

        static void AllocateTalentPoints(out int strength, out int intelligence, out int charisma,
                                         out int programming, out int art, out int sports, out int leadership, out int investmentSkill)
        {
            strength = intelligence = charisma = programming = art = sports = leadership = investmentSkill = 0;
            int basePool = 25;
            int skillPool = 20;

            GlobalState.PrintDivider("基础属性分配");
            string[] baseAttrs = { "力量", "智力", "魅力" };
            int[] baseValues = new int[baseAttrs.Length];
            int remaining = basePool;

            for (int i = 0; i < baseAttrs.Length; i++)
            {
                int min = 1;
                int max = remaining - (baseAttrs.Length - i - 1) * min;
                baseValues[i] = ReadAttributeWithRange(baseAttrs[i], min, max, remaining);
                remaining -= baseValues[i];
                Console.WriteLine($"【剩余点数: {remaining}】");
            }

            strength = baseValues[0];
            intelligence = baseValues[1];
            charisma = baseValues[2];

            GlobalState.PrintDivider("技能属性分配");
            string[] skillAttrs = { "编程", "艺术", "运动", "领导力", "投资技能" };
            int[] skillValues = new int[skillAttrs.Length];
            remaining = skillPool;

            for (int i = 0; i < skillAttrs.Length; i++)
            {
                int min = 0;
                int max = remaining;
                skillValues[i] = ReadAttributeWithRange(skillAttrs[i], min, max, remaining);
                remaining -= skillValues[i];
                Console.WriteLine($"【剩余点数: {remaining}】");
            }

            programming = skillValues[0];
            art = skillValues[1];
            sports = skillValues[2];
            leadership = skillValues[3];
            investmentSkill = skillValues[4];

            Console.WriteLine("\n属性分配完成！");
            Console.WriteLine($"力量:{strength} 智力:{intelligence} 魅力:{charisma}");
            Console.WriteLine($"编程:{programming} 艺术:{art} 运动:{sports} 领导力:{leadership} 投资:{investmentSkill}");
            Console.WriteLine(GlobalState.GetMessage("pressAnyKey"));
            Console.ReadKey();
        }

        static int ReadAttributeWithRange(string attributeName, int min, int max, int remaining)
        {
            int value;
            while (true)
            {
                Console.Write($"{attributeName} ({min}-{max}，剩余 {remaining} 点): ");
                string input = Console.ReadLine();

                if (int.TryParse(input, out value) && value >= min && value <= max)
                {
                    return value;
                }
                Console.WriteLine($"输入无效，请输入 {min} 到 {max} 之间的整数");
            }
        }
    }
    #endregion

    #region 存档/读档与可视化
    static class SaveLoadManager
    {
        public static void SaveGame(Character character, int slot)
        {
            string filename = $"life_save_{slot}.dat";
            try
            {
                using (StreamWriter sw = new StreamWriter(filename))
                {
                    sw.WriteLine(character.Generation);
                    sw.WriteLine(character.Age);
                    sw.WriteLine(character.Health);
                    sw.WriteLine(character.Strength);
                    sw.WriteLine(character.Intelligence);
                    sw.WriteLine(character.Charisma);
                    sw.WriteLine(character.Wealth);
                    sw.WriteLine(character.Happiness);
                    sw.WriteLine((int)character.CurrentCareer);
                    sw.WriteLine(character.Programming);
                    sw.WriteLine(character.Art);
                    sw.WriteLine(character.Sports);
                    sw.WriteLine(character.Leadership);
                    sw.WriteLine(character.InvestmentSkill);
                    sw.WriteLine(character.OriginalStrength);
                    sw.WriteLine(character.OriginalIntelligence);
                    sw.WriteLine(character.OriginalCharisma);
                    sw.WriteLine(character.OriginalProgramming);
                    sw.WriteLine(character.OriginalArt);
                    sw.WriteLine(character.OriginalSports);
                    sw.WriteLine(character.OriginalLeadership);
                    sw.WriteLine(character.OriginalInvestmentSkill);
                    sw.WriteLine(character.HasHereditaryDisease);
                    sw.WriteLine(character.ChronicDiseaseLevel);
                    sw.WriteLine(character.MajorDiseaseCount);
                    sw.WriteLine(string.Join("|", character.Achievements));
                    foreach (var rel in character.Relationships)
                    {
                        sw.WriteLine($"{rel.Key}:{rel.Value}");
                    }
                    sw.WriteLine(string.Join("|", character.LifeEvents));
                    sw.WriteLine(string.Join("|", character.Milestones));
                }
                Console.WriteLine($"{GlobalState.GetMessage("gameSaved")} {filename}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存失败: {ex.Message}");
            }
        }

        public static Character LoadGame(int slot)
        {
            string filename = $"life_save_{slot}.dat";
            try
            {
                using (StreamReader sr = new StreamReader(filename))
                {
                    int generation = int.Parse(sr.ReadLine());
                    Character character = new Character(5, 5, 5, 0, 0, 0, 0, 0) { Generation = generation };

                    character.Age = int.Parse(sr.ReadLine());
                    character.Health = int.Parse(sr.ReadLine());
                    character.Strength = int.Parse(sr.ReadLine());
                    character.Intelligence = int.Parse(sr.ReadLine());
                    character.Charisma = int.Parse(sr.ReadLine());
                    character.Wealth = int.Parse(sr.ReadLine());
                    character.Happiness = int.Parse(sr.ReadLine());
                    character.CurrentCareer = (Career)int.Parse(sr.ReadLine());
                    character.Programming = int.Parse(sr.ReadLine());
                    character.Art = int.Parse(sr.ReadLine());
                    character.Sports = int.Parse(sr.ReadLine());
                    character.Leadership = int.Parse(sr.ReadLine());
                    character.InvestmentSkill = int.Parse(sr.ReadLine());
                    character.OriginalStrength = int.Parse(sr.ReadLine());
                    character.OriginalIntelligence = int.Parse(sr.ReadLine());
                    character.OriginalCharisma = int.Parse(sr.ReadLine());
                    character.OriginalProgramming = int.Parse(sr.ReadLine());
                    character.OriginalArt = int.Parse(sr.ReadLine());
                    character.OriginalSports = int.Parse(sr.ReadLine());
                    character.OriginalLeadership = int.Parse(sr.ReadLine());
                    character.OriginalInvestmentSkill = int.Parse(sr.ReadLine());
                    character.HasHereditaryDisease = bool.Parse(sr.ReadLine());
                    character.ChronicDiseaseLevel = int.Parse(sr.ReadLine());
                    character.MajorDiseaseCount = int.Parse(sr.ReadLine());

                    string achievements = sr.ReadLine();
                    if (!string.IsNullOrEmpty(achievements))
                        character.Achievements = new List<string>(achievements.Split('|'));

                    string line;
                    while ((line = sr.ReadLine()) != null && !line.Contains("|"))
                    {
                        var parts = line.Split(':');
                        if (parts.Length == 2)
                            character.Relationships[parts[0]] = int.Parse(parts[1]);
                    }

                    if (line != null)
                    {
                        character.LifeEvents = new List<string>(line.Split('|'));
                        line = sr.ReadLine();
                        if (line != null)
                            character.Milestones = new List<string>(line.Split('|'));
                    }

                    return character;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{GlobalState.GetMessage("loadFailed")} {ex.Message}");
                return null;
            }
        }
    }

    static class Visualization
    {
        public static void ShowAsciiChart(Character character)
        {
            Console.WriteLine("╔════════════════════════════════╗");
            Console.WriteLine("║         📊 人生状态图         ║");
            Console.WriteLine("╠════════════════════════════════╣");
            Console.WriteLine($"║ 健康: {GetBar(character.Health, 100)}");
            Console.WriteLine($"║ 幸福: {GetBar(character.Happiness, 100)}");
            Console.WriteLine($"║ 财富: {GetBar(character.Wealth, 50000)}");
            Console.WriteLine($"║ 事业: {GetCareerBar(character.CurrentCareer)}");
            Console.WriteLine("╚════════════════════════════════╝");
        }

        private static string GetBar(int value, int max)
        {
            int length = 20;
            int filled = (int)Math.Round((double)value / max * length);
            filled = Math.Min(filled, length);
            return $"{new string('█', filled)}{new string('░', length - filled)} {value}";
        }

        private static string GetCareerBar(Career career)
        {
            int careerLevel = (int)career;
            int maxLevel = (int)Career.CEO;
            return GetBar(careerLevel * 2, maxLevel * 2);
        }
    }
    #endregion
}