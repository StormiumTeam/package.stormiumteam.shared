using System;
using System.Globalization;
using System.Net;

namespace Utilities
{
	public static class IPEndPointUtility
	{
		public static bool TryParse(string s, out IPEndPoint result)
		{
			return TryParse(s.AsSpan(), out result);
		}

		public static bool TryParse(ReadOnlySpan<char> s, out IPEndPoint result)
		{   
			int addressLength = s.Length; // If there's no port then send the entire string to the address parser
			int lastColonPos  = s.LastIndexOf(':');

			// Look to see if this is an IPv6 address with a port.
			if (lastColonPos > 0)
			{
				if (s[lastColonPos - 1] == ']')
				{
					addressLength = lastColonPos;
				}
				// Look to see if this is IPv4 with a port (IPv6 will have another colon)
				else if (s.Slice(0, lastColonPos).LastIndexOf(':') == -1)
				{
					addressLength = lastColonPos;
				}
			}

			if (IPAddress.TryParse(s.Slice(0, addressLength).ToString(), out IPAddress address))
			{
				uint port = 0;
				if (addressLength == s.Length ||
				    (uint.TryParse(s.Slice(addressLength + 1).ToString(), NumberStyles.None, CultureInfo.InvariantCulture, out port) && port <= IPEndPoint.MaxPort))
                    
				{
					result = new IPEndPoint(address, (int)port);
					return true;
				}
			}

			result = null;
			return false;
		}

		public static IPEndPoint Parse(string s)
		{
			if (s == null)
			{
				throw new ArgumentNullException(nameof(s));
			}

			return Parse(s.AsSpan());
		}

		public static IPEndPoint Parse(ReadOnlySpan<char> s)
		{
			if (TryParse(s, out IPEndPoint result))
			{
				return result;
			}

			throw new FormatException();
		}
	}
}