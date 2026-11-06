using System;

namespace TopDownGame
{
    public static class FirebaseConfig
    {
        public const string ApiKey = "AIzaSyDdgDaVeb-WSYr_t8EOJQhEDN3lmcSPQD0";
        public const string AuthDomain = "pamana-bcf1d.firebaseapp.com";
        public const string DatabaseUrl = "https://pamana-bcf1d-default-rtdb.firebaseio.com";
        public const string ProjectId = "pamana-bcf1d";
        public const string AppId = "1:704187323540:web:a07a9a90975160b88d5081";

        // REST Endpoints
        public static string SignUpUrl => $"https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={ApiKey}";
        public static string SignInUrl => $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={ApiKey}";
        public static string ResetPasswordUrl => $"https://identitytoolkit.googleapis.com/v1/accounts:sendOobCode?key={ApiKey}";

        /// <summary>
        /// Formats an input string into a valid email. If the user entered just a username (e.g. 'Lance'),
        /// we format it as 'lance@pamana.game' for seamless username-based login via Firebase Auth.
        /// </summary>
        public static string FormatEmail(string emailOrUsername)
        {
            if (string.IsNullOrWhiteSpace(emailOrUsername)) return string.Empty;
            string trimmed = emailOrUsername.Trim();
            if (trimmed.Contains("@")) return trimmed;
            return $"{trimmed.ToLowerInvariant()}@pamana.game";
        }

        public static string ExtractUsername(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return "Player";
            if (email.EndsWith("@pamana.game", StringComparison.OrdinalIgnoreCase))
            {
                return email.Substring(0, email.IndexOf('@'));
            }
            int atIdx = email.IndexOf('@');
            return atIdx > 0 ? email.Substring(0, atIdx) : email;
        }
    }
}

