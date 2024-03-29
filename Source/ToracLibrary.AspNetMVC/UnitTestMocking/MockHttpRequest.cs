﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ToracLibrary.AspNet.AspNetMVC.UnitTestMocking
{

    /// <summary>
    ///  Class used to mock a HttpRequestBase
    /// </summary>
    public class MockHttpRequest : HttpRequestBase
    {

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="RelativeUrlToSet">Relative Url</param>
        /// <param name="FormParamsToSet">Form Params</param>
        /// <param name="QueryStringParamsToSet">Query String Parms</param>
        /// <param name="CookiesToSet">Cookies</param>
        /// <param name="ContentTypeToSet">Content type to set</param>
        /// <param name="InputStreamToSet">Input stream to set</param>
        public MockHttpRequest(string RelativeUrlToSet, NameValueCollection FormParamsToSet, NameValueCollection QueryStringParamsToSet, HttpCookieCollection CookiesToSet, string ContentTypeToSet, Stream InputStreamToSet)
        {
            RelativeUrl = RelativeUrlToSet;
            FormParams = FormParamsToSet;
            QueryStringParams = QueryStringParamsToSet;
            CookieParams = CookiesToSet;
            ContentType = ContentTypeToSet;
            InputStream = InputStreamToSet;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Relative Url
        /// </summary>
        private string RelativeUrl { get; }

        /// <summary>
        /// Form params
        /// </summary>
        private NameValueCollection FormParams { get; }

        /// <summary>
        /// Query string
        /// </summary>
        private NameValueCollection QueryStringParams { get; }

        /// <summary>
        /// Cookies
        /// </summary>
        private HttpCookieCollection CookieParams { get; }

        #endregion

        #region Override HttpRequestBase Methods

        /// <summary>
        /// Form Params
        /// </summary>
        public override NameValueCollection Form
        {
            get { return FormParams; }
        }
        /// <summary>
        /// Query string
        /// </summary>
        public override NameValueCollection QueryString
        {
            get { return QueryStringParams; }
        }

        /// <summary>
        /// Cookies
        /// </summary>
        public override HttpCookieCollection Cookies
        {
            get { return CookieParams; }
        }

        /// <summary>
        /// Relative Url
        /// </summary>
        public override string AppRelativeCurrentExecutionFilePath
        {
            get { return RelativeUrl; }
        }

        /// <summary>
        /// Path Info
        /// </summary>
        public override string PathInfo
        {
            get { return string.Empty; }
        }

        /// <summary>
        /// Content type of the request
        /// </summary>
        public override string ContentType { get; set; }

        /// <summary>
        /// Input stream to mock
        /// </summary>
        public override Stream InputStream { get; }

        #endregion

    }

}
