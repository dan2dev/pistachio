using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Pistachio {
	public class PaginatedList<T> where T : IEntity, new() {
		public int Page { get; set; }
		public int RowsByPage { get; set; }
		public int Count { get; set; }
		public int Skip { get; set; }
		public int PageCount {
			get {
				var pageCount = Math.Ceiling(Convert.ToDecimal(Count) / RowsByPage);
				return Convert.ToInt32(pageCount);
			}
		}
		public int NextPage {
			get {
				if (Page < PageCount && Page >= 1) {
					return Page + 1;
				} else {
					return -1;
				}
			}
		}
		public int PrevPage {
			get {
				if (Page > 1 && Page <= PageCount) {
					return Page - 1;
				} else {
					return -1;
				}
			}
		}
		#region neighbors pages
		private int neighborPagesCount = 3;
		public int NeighborPagesCount {
			get {
				return neighborPagesCount;
			}
			set {
				neighborPagesCount = value;
			}
		}
		public List<int> PrevPages {
			get {
				List<int> pages = new List<int>();
				for (int i = Page - 1;
					i >= Page - neighborPagesCount &&
					i >= 1;
				i--) {
					pages.Add(i);
				}
				pages.Reverse();
				return pages;
			}
		}
		public List<int> NextPages {
			get {
				List<int> pages = new List<int>();
				for (int i = Page + 1;
					i <= Page + neighborPagesCount &&
					i <= PageCount;
				i++) {
					pages.Add(i);
				}
				return pages;
			}
		}
		#endregion
		public List<T> Items = new List<T>();
		#region
		private static Regex rgx = new Regex(@"(p=\d+)", RegexOptions.ECMAScript);
		public string PageUrl { get; set; } = null;
		public string NextPageUrl { get; set; } = null;
		public string PrevPageUrl { get; set; } = null;
		public string PageUrlTemplate { get; set; } = null;
		public void SetPageUrl(string currentUrlWithQueryString) {
			if (!currentUrlWithQueryString.Contains("?")) {
				PageUrl = currentUrlWithQueryString + "?p=" + Page;
			} else {
				if (rgx.Match(currentUrlWithQueryString).Success) {
					PageUrl = rgx.Replace(currentUrlWithQueryString, ("p=" + Page));
				} else {
					PageUrl = currentUrlWithQueryString + ("&p=" + Page);
				}
			}
			if(NextPage >= 1) {
				NextPageUrl = rgx.Replace(PageUrl, ("p=" + NextPage.ToString())); 
			}
			if(PrevPage >= 1) {
				PrevPageUrl = rgx.Replace(PageUrl, ("p=" + PrevPage.ToString()));
			}
			PageUrlTemplate = rgx.Replace(PageUrl, ("p={{page}}"));
		}
		#endregion
	}
}
