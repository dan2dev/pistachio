using System;
using System.Collections.Generic;
using System.Text;

namespace Pistachio.Helpers {
	public static partial class Helper {
		public static string ToPascalCase(this string value) {
			var splited = Split(value);
			string returnValue = "";
			foreach (var part in splited) {
				returnValue += UppercaseFirstLetter(part);
			}
			return returnValue;
		}
		public static string ToCamelCase(this string value) {
			var splited = Split(value);
			string returnValue = "";
			for (int i = 0; i < splited.Length; i++) {
				string part = splited[i];
				if (i == 0) {
					returnValue += part.ToLowerInvariant();
				} else {
					returnValue += part.UppercaseFirstLetter();
				}
			}
			return returnValue;
		}
		private static string[] Split(string value) {
			return value.Replace("_", "-").Replace(".", "-").Split('-');
		}
		public static string UppercaseFirstLetter(this string value) {
			if (string.IsNullOrWhiteSpace(value) || value.Length <= 1) {
				return value.ToUpperInvariant();
			} else {
				return char.ToUpperInvariant(value[0]) + value.Substring(1).ToLowerInvariant();
			}
		}
	}
}