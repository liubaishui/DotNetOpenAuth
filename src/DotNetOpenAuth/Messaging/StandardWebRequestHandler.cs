﻿//-----------------------------------------------------------------------
// <copyright file="StandardWebRequestHandler.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System;
	using System.IO;
	using System.Net;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// The default handler for transmitting <see cref="HttpWebRequest"/> instances
	/// and returning the responses.
	/// </summary>
	internal class StandardWebRequestHandler : IDirectWebRequestHandler {
		#region IWebRequestHandler Members

		/// <summary>
		/// Prepares a POST <see cref="HttpWebRequest"/> and returns the request stream 
		/// for writing out the POST entity data.
		/// </summary>
		/// <param name="request">The <see cref="HttpWebRequest"/> that should contain the entity.</param>
		/// <returns>The stream the caller should write out the entity data to.</returns>
		public TextWriter GetRequestStream(HttpWebRequest request) {
			ErrorUtilities.VerifyArgumentNotNull(request, "request");

			try {
				return new StreamWriter(request.GetRequestStream());
			} catch (WebException ex) {
				throw new ProtocolException(MessagingStrings.ErrorInRequestReplyMessage, ex);
			}
		}

		/// <summary>
		/// Processes an <see cref="HttpWebRequest"/> and converts the 
		/// <see cref="HttpWebResponse"/> to a <see cref="DirectWebResponse"/> instance.
		/// </summary>
		/// <param name="request">The <see cref="HttpWebRequest"/> to handle.</param>
		/// <returns>An instance of <see cref="DirectWebResponse"/> describing the response.</returns>
		public DirectWebResponse GetResponse(HttpWebRequest request) {
			ErrorUtilities.VerifyArgumentNotNull(request, "request");

			try {
				Logger.DebugFormat("HTTP {0} {1}", request.Method, request.RequestUri);
				using (HttpWebResponse response = (HttpWebResponse)request.GetResponse()) {
					return new DirectWebResponse(request.RequestUri, response);
				}
			} catch (WebException ex) {
				if (Logger.IsErrorEnabled) {
					if (ex.Response != null) {
						using (var reader = new StreamReader(ex.Response.GetResponseStream())) {
							Logger.ErrorFormat("WebException from {0}: {1}", ex.Response.ResponseUri, reader.ReadToEnd());
						}
					} else {
						Logger.ErrorFormat("WebException {1} from {0}, no response available.", request.RequestUri, ex.Status);
					}
				}
				throw new ProtocolException(MessagingStrings.ErrorInRequestReplyMessage, ex);
			}
		}

		#endregion
	}
}
