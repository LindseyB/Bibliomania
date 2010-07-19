using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Net;
using System.IO;
using System.Windows.Forms;
using OAuth;

namespace WindowsFormsApplication1 {
	// Class for making OAuth Requests
	class OAuth {
		private string oauthToken;
		private string oauthTokenSecret;
		private string consumerKey;
		private string consumerSecret;

		public OAuth() {
			consumerKey = "eStfu8ZZXxins2tOdlVjUA";
			consumerSecret = "A773YHERJAWODRasHREh9GOjFTEuGdGQvu1cfbXFfM";  // shhh! It's a secret
			oauthToken = Properties.Settings.Default.OAuthToken;
			oauthTokenSecret = Properties.Settings.Default.OAuthTokenSecret;
		}

		// gets the oauth token if we do not have one already 
		public void getOAuthToken() {
			OAuthBase oAuth = new OAuthBase();
			string nonce, normalizedUrl, normalizedRequestParameters, sig, timeStamp;
			Uri uri;

			// TODO: verify that the tokens we currently have are active and permission has not been revoked for the app
			if (Properties.Settings.Default.OAuthToken.Equals("") || Properties.Settings.Default.Equals("")) {
				uri = new Uri("http://www.goodreads.com/oauth/request_token");

				nonce = oAuth.GenerateNonce();
				timeStamp = oAuth.GenerateTimeStamp();
				sig = oAuth.GenerateSignature(uri, consumerKey, consumerSecret, null, null, "GET", timeStamp, nonce, out normalizedUrl, out normalizedRequestParameters);

				sig = HttpUtility.UrlEncode(sig);

				string request_url = normalizedUrl + "?" + normalizedRequestParameters + "&oauth_signature=" + sig;

				oauthTokenReq(request_url, out oauthToken, out oauthTokenSecret);

				// go get authorized
				uri = new Uri("http://www.goodreads.com/oauth/authorize");
				string oauthURL = uri.ToString() + "?oauth_token=" + oauthToken;
				// open a browser and allow the user to authorize 
				System.Diagnostics.Process.Start(oauthURL);

				// Instead of sleeping prompt the user to verify that they entered their credentials before proceeding to the next step
				if (MessageBox.Show("Did you allow Bibliomania access?", "Confirm Access", MessageBoxButtons.YesNo) == DialogResult.No) {
					Properties.Settings.Default.OAuthToken = "";
					Properties.Settings.Default.OAuthTokenSecret = "";
					Properties.Settings.Default.Save();
					return;
				}

				uri = new Uri("http://www.goodreads.com/oauth/access_token");
				nonce = oAuth.GenerateNonce();
				timeStamp = oAuth.GenerateTimeStamp();
				// this time we need our oauth token and oauth token secret
				sig = oAuth.GenerateSignature(uri, consumerKey, consumerSecret, oauthToken, oauthTokenSecret, "GET", timeStamp, nonce, out normalizedUrl, out normalizedRequestParameters);
				sig = HttpUtility.UrlEncode(sig);

				// notice that the sig is always being appended to the end
				string accessUrl = normalizedUrl + "?" + normalizedRequestParameters + "&oauth_signature=" + sig;

				oauthTokenReq(accessUrl, out oauthToken, out oauthTokenSecret);
				// store these in the settings
				Properties.Settings.Default.OAuthToken = oauthToken;
				Properties.Settings.Default.OAuthTokenSecret = oauthTokenSecret;
				Properties.Settings.Default.Save();
			}
		}

		// generates the url for accessing data that we need to use OAuth to access
		public string getOAuthDataUrl(string url) {
			OAuthBase oAuth = new OAuthBase();
			Uri uri = new Uri(url);
			string nonce = oAuth.GenerateNonce();
			string timeStamp = oAuth.GenerateTimeStamp();
			string normalizedUrl, normalizedRequestParameters;

			string sig = oAuth.GenerateSignature(uri, consumerKey, consumerSecret, oauthToken, oauthTokenSecret, "GET", timeStamp, nonce, out normalizedUrl, out normalizedRequestParameters);
			sig = HttpUtility.UrlEncode(sig);
			return normalizedUrl + "?" + normalizedRequestParameters + "&oauth_signature=" + sig;
		}

		// helper function for requesting the token from the server
		private void oauthTokenReq(string url, out string oauthTok, out string oauthTokSecret) {
			HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(url);
			StreamReader stIn;

			try {
				stIn = new StreamReader(httpRequest.GetResponse().GetResponseStream());
			} catch (Exception e) {
				Console.WriteLine(e.Message);
				oauthTok = "";
				oauthTokSecret = "";
				return;
			}

			string strResponse = stIn.ReadToEnd();
			stIn.Close();

			oauthTok = strResponse.Substring(12, strResponse.IndexOf('&') - 12);
			oauthTokSecret = strResponse.Substring(strResponse.IndexOf('&') + 20);
		}
	}
}
