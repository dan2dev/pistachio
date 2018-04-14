using System;
using Pistachio.Reflection;

namespace Pistachio {
	public class Ide {
		private string hash;
		public string Hash {
			get {
				if (hash == null)
					NewHash();
				return hash;
			}
			set {
				this.hash = value;
			}
		}
		private void NewHash() {
			Guid guid = System.Guid.NewGuid();
			this.Hash = DataReflection.CreateMD5(guid.ToString()).ToString();
		}
		public static implicit operator Ide(string value) {
			return new Ide() { Hash = value };
		}

		public static implicit operator string(Ide value) {
			return value.Hash;
		}

		public override string ToString() {
			return Hash;
		}
	}
}
