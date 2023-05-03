/**
 * Copyright (c) 2021 The Campfire Union Inc - All Rights Reserved.
 *
 * Unauthorized copying of this file, via any medium, is strictly prohibited.
 * This source code is proprietary and confidential.
 *
 * Email:   info@campfireunion.com
 * Website: https://www.campfireunion.com
 */

using System;
using System.Net;
using System.Net.Sockets;

namespace Flipside.Helpers {

	public class NetworkInfo {

		public static string IPAddress () {
			IPHostEntry host = Dns.GetHostEntry (Dns.GetHostName ());

			foreach (IPAddress ip in host.AddressList) {
				if (ip.AddressFamily == AddressFamily.InterNetwork) {
					return ip.ToString ();
				}
			}

			return "IP address not found";
		}
	}
}