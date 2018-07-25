using System;
using System.Net.Sockets;

namespace io.netty.resolver
{
	public class InetAddress
	{
		public class InetAddressHolder
		{
			/**
			 * Reserve the original application specified hostname.
			 *
			 * The original hostname is useful for domain-based endpoint
			 * identification (see RFC 2818 and RFC 6125).  If an address
			 * was created with a raw IP address, a reverse name lookup
			 * may introduce endpoint identification security issue via
			 * DNS forging.
			 *
			 * Oracle JSSE provider is using this original hostname, via
			 * sun.misc.JavaNetAccess, for SSL/TLS endpoint identification.
			 *
			 * Note: May define a new public method in the future if necessary.
			 */
			String originalHostName;

			InetAddressHolder() { }

			InetAddressHolder(String hostName, int address, AddressFamily family)
			{
				this.originalHostName = hostName;
				this.hostName = hostName;
				this.address = address;
				this.family = family;
			}

			void init(String hostName, AddressFamily family)
			{
				this.originalHostName = hostName;
				this.hostName = hostName;
				if (family != AddressFamily.Unknown)
				{
					this.family = family;
				}
			}

			public String hostName;

			public String getHostName()
			{
				return hostName;
			}

			String getOriginalHostName()
			{
				return originalHostName;
			}

			/**
			 * Holds a 32-bit IPv4 address.
			 */
			int address;

			int getAddress()
			{
				return address;
			}

			/**
			 * Specifies the address family type, for instance, '1' for IPv4
			 * addresses, and '2' for IPv6 addresses.
			 */
			AddressFamily family;

			AddressFamily getFamily()
			{
				return family;
			}
		}

		private InetAddressHolder _holder = null;

		public InetAddressHolder holder()
		{
			return _holder;
		}

		public String getHostName()
		{
			return getHostName(true);
		}

		String getHostName(bool check)
		{
			if (holder().getHostName() == null)
			{
				holder().hostName = InetAddress.getHostFromNameService(this, check);
			}
			return holder().getHostName();
		}

		private static String getHostFromNameService(InetAddress addr, bool check)
		{
// 			String host = null;
// 			foreach (NameService nameService in nameServices)
// 			{
// 				try
// 				{
// 					// first lookup the hostname
// 					host = nameService.getHostByAddr(addr.getAddress());
// 
// 					/* check to see if calling code is allowed to know
// 					 * the hostname for this IP address, ie, connect to the host
// 					 */
// 					if (check)
// 					{
// 						SecurityManager sec = System.getSecurityManager();
// 
// 						if (sec != null)
// 						{
// 							sec.checkConnect(host, -1);
// 						}
// 					}
// 
// 					/* now get all the IP addresses for this hostname,
// 					 * and make sure one of them matches the original IP
// 					 * address. We do this to try and prevent spoofing.
// 					 */
// 					InetAddress[] arr = InetAddress.getAllByName0(host, check);
// 					bool ok = false;
// 
// 					if (arr != null)
// 					{
// 						for (int i = 0; !ok && i < arr.Length; i++)
// 						{
// 							ok = addr.Equals(arr[i]);
// 						}
// 					}
// 
// 					//XXX: if it looks a spoof just return the address?
// 					if (!ok)
// 					{
// 						host = addr.getHostAddress();
// 						return host;
// 					}
// 
// 					break;
// 
// 				}
// 				catch (SecurityException e)
// 				{
// 					host = addr.getHostAddress();
// 					break;
// 				}
// 				catch (UnknownHostException e)
// 				{
// 					host = addr.getHostAddress();
// 					// let next provider resolve the hostname
// 				}
// 			}
// 
// 			return host;
			return null;
		}

		public byte[] getAddress()
		{
			return null;
		}

		public String getHostAddress()
		{
			return null;
		}

		public String toString()
		{
			String hostName = holder().getHostName();
			return ((hostName != null) ? hostName : "") + "/" + getHostAddress();
		}

		static InetAddress[] getAllByName0(String host, bool check)
		{
			return getAllByName0(host, null, check);
		}

		private static InetAddress[] getAllByName0(String host, InetAddress reqAddr, bool check)
		{

			/* If it gets here it is presumed to be a hostname */
			/* Cache.get can return: null, unknownAddress, or InetAddress[] */

			/* make sure the connection to the host is allowed, before we
			 * give out a hostname
			 */
// 			if (check)
// 			{
// 				SecurityManager security = System.getSecurityManager();
// 				if (security != null)
// 				{
// 					security.checkConnect(host, -1);
// 				}
// 			}
// 
// 			InetAddress[] addresses = getCachedAddresses(host);
// 
// 			/* If no entry in cache, then do the host lookup */
// 			if (addresses == null)
// 			{
// 				addresses = getAddressesFromNameService(host, reqAddr);
// 			}
// 
// 			if (addresses == unknown_array)
// 				throw new UnknownHostException(host);
// 
// 			return addresses.clone();
			return null;
		}
	}
}
