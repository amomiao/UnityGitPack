using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Momos.UnityGitPack.Common {
    [Serializable]
    public class UPMData {
        [Serializable]
        public class AuthorData {
            public string name;
            public string email;
        }

        [Serializable]
        public class DependencyData {
            public string name;        // com.xxx.yyy
            public string reference;   
        }

        public string name;
        public string version;
        public string displayName;
        public string description;
        public string unity;

        public List<AuthorData> author = new List<AuthorData>();
        public List<DependencyData> dependencies = new List<DependencyData>();

        #region IJsonWritable
        public string ToJson() {
            return JsonUtility.ToJson(this);

            var sb = new StringBuilder(512);
            sb.AppendLine("{");

            AppendField(sb, "name", name, true);
            AppendField(sb, "version", version, true);
            AppendField(sb, "displayName", displayName, true);
            AppendField(sb, "description", description, true);
            AppendField(sb, "unity", unity, HasAuthor() || HasDependencies());

            if (HasAuthor()) {
                sb.AppendLine("  \"author\": " + BuildAuthorJson() + (HasDependencies() ? "," : ""));
            }

            if (HasDependencies()) {
                sb.AppendLine("  \"dependencies\": {");
                AppendDependencies(sb);
                sb.AppendLine("  }");
            }

            sb.AppendLine("}");
            return sb.ToString();
        }

        #region Build Helpers
        private bool HasAuthor() {
            return author != null && author.Count > 0;
        }

        private bool HasDependencies() {
            if (dependencies == null) return false;
            foreach (var d in dependencies) {
                if (!string.IsNullOrEmpty(d.name) &&
                    !string.IsNullOrEmpty(d.reference))
                    return true;
            }
            return false;
        }

        private void AppendDependencies(StringBuilder sb) {
            bool first = true;
            foreach (var dep in dependencies) {
                if (string.IsNullOrEmpty(dep.name) ||
                    string.IsNullOrEmpty(dep.reference))
                    continue;

                if (!first)
                    sb.AppendLine(",");

                sb.Append("    \"")
                  .Append(Escape(dep.name))
                  .Append("\": \"")
                  .Append(Escape(dep.reference))
                  .Append("\"");

                first = false;
            }
            sb.AppendLine();
        }

        private string BuildAuthorJson() {
            if (author.Count == 1)
                return BuildAuthorObject(author[0]);

            var sb = new StringBuilder();
            sb.AppendLine("[");
            for (int i = 0; i < author.Count; i++) {
                sb.Append("    ")
                  .Append(BuildAuthorObject(author[i]));

                if (i < author.Count - 1)
                    sb.Append(",");

                sb.AppendLine();
            }
            sb.Append("  ]");
            return sb.ToString();
        }

        private static string BuildAuthorObject(AuthorData a) {
            var sb = new StringBuilder();
            sb.Append("{ ");

            bool comma = false;
            if (!string.IsNullOrEmpty(a.name)) {
                sb.Append("\"name\": \"").Append(Escape(a.name)).Append("\"");
                comma = true;
            }

            if (!string.IsNullOrEmpty(a.email)) {
                if (comma) sb.Append(", ");
                sb.Append("\"email\": \"").Append(Escape(a.email)).Append("\"");
            }

            sb.Append(" }");
            return sb.ToString();
        }

        private static void AppendField(
            StringBuilder sb,
            string key,
            string value,
            bool comma) {
            if (string.IsNullOrEmpty(value))
                return;

            sb.Append("  \"")
              .Append(key)
              .Append("\": \"")
              .Append(Escape(value))
              .Append("\"");

            if (comma)
                sb.Append(",");

            sb.AppendLine();
        }

        private static string Escape(string value) {
            if (string.IsNullOrEmpty(value))
                return value;

            return value
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace("\t", "\\t");
        }

        #endregion

        #endregion
    }
}
