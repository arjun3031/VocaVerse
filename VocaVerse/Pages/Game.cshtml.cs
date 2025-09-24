
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace VocaVerse.Pages
{
    public class GameModel : PageModel
    {
        // Game state properties
        public string LastName { get; set; } = "";
        public HashSet<string> UsedNames { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public string CurrentPlayer { get; set; } = "Player 1";
        public bool GameStarted { get; set; } = false;
        public string Message { get; set; } = "Player 1, enter the first name to start the game!";

        // Predefined list of valid 5-letter Indian names
        private static readonly HashSet<string> ValidNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Arjun", "Aisha", "Rohan", "Neha", "Kiran", "Priya", "Rahul", "Sonia",
            "Vikram", "Meera", "Ashok", "Kavya", "Sunil", "Riya", "Manoj", "Pooja",
            "Deepak", "Anita", "Rajesh", "Shreya", "Amit", "Divya", "Suresh", "Nisha",
            "Gaurav", "Rekha", "Mohit", "Sunita", "Anil", "Geeta", "Vivek", "Seema",
            "Prakash", "Lata", "Ramesh", "Maya", "Santosh", "Usha", "Ajay", "Radha",
            "Naresh", "Kiran", "Dinesh", "Shanta", "Yogesh", "Poonam", "Hitesh", "Suman",
            "Mahesh", "Indu", "Jatin", "Sushma", "Lalit", "Prema", "Nitin", "Sunanda",
            "Pavan", "Nanda", "Raman", "Sudha", "Kiran", "Leela", "Mohan", "Kamala",
            "Niraj", "Jaya", "Sachin", "Sarla", "Tarun", "Nirmala", "Varun", "Urmila",
            "Yatin", "Nalini", "Parag", "Gitika", "Samir", "Rajni", "Tushar", "Swati",
            "Umang", "Garima", "Vishal", "Lalita", "Wasim", "Mamta", "Xitij", "Namita",
            "Yuvraj", "Omprakash", "Zubin", "Anuja", "Abhay", "Beena", "Chirag", "Deepa",
            "Eshan", "Farhan", "Harsh", "Indira", "Jagan", "Kunal", "Lucky", "Minal",
            "Nikhil", "Omkar", "Priyank", "Queen", "Rishi", "Sagar", "Tejas", "Ujjwal"
        };

        public void OnGet()
        {
            LoadGameState();
        }

        public IActionResult OnPostPlay()
        {
            LoadGameState();

            string inputName = Request.Form["name"].ToString().Trim();

            // Validate input
            if (string.IsNullOrEmpty(inputName))
            {
                Message = "Please enter a name!";
                return new JsonResult(GetGameState());
            }

            if (inputName.Length != 5)
            {
                Message = "Name must be exactly 5 letters long!";
                return new JsonResult(GetGameState());
            }

            if (!ValidNames.Contains(inputName))
            {
                Message = "Invalid name! Please enter a valid 5-letter Indian name.";
                return new JsonResult(GetGameState());
            }

            if (UsedNames.Contains(inputName))
            {
                Message = "This name has already been used! Try a different name.";
                return new JsonResult(GetGameState());
            }

            // Check word chain rule (except for first name)
            if (GameStarted && !inputName.StartsWith(LastName.Last().ToString(), StringComparison.OrdinalIgnoreCase))
            {
                Message = $"Name must start with '{LastName.Last()}' (the last letter of '{LastName}')!";
                return new JsonResult(GetGameState());
            }

            // Valid move - update game state
            LastName = inputName;
            UsedNames.Add(inputName);
            GameStarted = true;

            // Switch player
            CurrentPlayer = CurrentPlayer == "Player 1" ? "Player 2" : "Player 1";
            Message = $"Great move! {CurrentPlayer}, your turn!";

            SaveGameState();
            return new JsonResult(GetGameState());
        }

        public IActionResult OnPostRestart()
        {
            // Reset all game state
            LastName = "";
            UsedNames.Clear();
            CurrentPlayer = "Player 1";
            GameStarted = false;
            Message = "Player 1, enter the first name to start the game!";

            SaveGameState();
            return new JsonResult(GetGameState());
        }

        private void LoadGameState()
        {
            if (HttpContext.Session.Keys.Contains("GameState"))
            {
                var gameStateJson = HttpContext.Session.GetString("GameState");
                if (!string.IsNullOrEmpty(gameStateJson))
                {
                    var gameState = JsonSerializer.Deserialize<GameStateModel>(gameStateJson);
                    LastName = gameState.LastName;
                    UsedNames = new HashSet<string>(gameState.UsedNames, StringComparer.OrdinalIgnoreCase);
                    CurrentPlayer = gameState.CurrentPlayer;
                    GameStarted = gameState.GameStarted;
                    Message = gameState.Message;
                }
            }
        }

        private void SaveGameState()
        {
            var gameState = new GameStateModel
            {
                LastName = LastName,
                UsedNames = UsedNames.ToList(),
                CurrentPlayer = CurrentPlayer,
                GameStarted = GameStarted,
                Message = Message
            };

            var gameStateJson = JsonSerializer.Serialize(gameState);
            HttpContext.Session.SetString("GameState", gameStateJson);
        }

        private object GetGameState()
        {
            return new
            {
                lastName = LastName,
                usedNames = UsedNames.ToList(),
                currentPlayer = CurrentPlayer,
                gameStarted = GameStarted,
                message = Message
            };
        }

        public class GameStateModel
        {
            public string LastName { get; set; } = "";
            public List<string> UsedNames { get; set; } = new List<string>();
            public string CurrentPlayer { get; set; } = "Player 1";
            public bool GameStarted { get; set; } = false;
            public string Message { get; set; } = "";
        }
    }
}
