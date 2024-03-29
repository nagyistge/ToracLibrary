﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Schema;
using ToracLibrary.Core.Xml.Schema;

namespace ToracLibrary.Core.Countries
{
    /// <summary>
    /// Retrieve all the country codes
    /// </summary>
    public class Country
    {

        #region Public Methods

        /// <summary>
        /// Get the country xml resouce
        /// </summary>
        /// <returns>xml file in an xdocument</returns>
        public static XDocument CountryXmlResource()
        {
            return XDocument.Parse(Properties.Resources.CountriesXml);
        }

        /// <summary>
        /// Get the country xml schema resource to validate with
        /// </summary>
        /// <returns>XmlSchemaSet to validate with</returns>
        public static XmlSchemaSet CountryXmlSchemaResource()
        {
            return XMLSchemaValidation.LoadSchemaFromText(Properties.Resources.CountriesSchema, null);
        }

        /// <summary>
        /// Get a dictionary of countries.
        /// </summary>
        /// <returns>IImmutableDictionary of int (country id) - Country Code Info</returns>
        /// <remarks>Is validated before returning data. It will raise any errors if there were errors found</remarks>
        public static IImmutableDictionary<int, CountryCodeInfo> CountryListing()
        {
            //xml is validated in a unit test against the schema. We don't need to keep validating it on each method call. This will make the method faster
            //because this is a really small xml document, an xml reader is actually slower!

            //dictionary to be returned
            var ReturnObject = new Dictionary<int, CountryCodeInfo>();

            //Loop Through The XML To Load The Dictionary
            foreach (XElement CountryToLoad in CountryXmlResource().Element("Countries").Elements("Country"))
            {
                //create the new country. Using a variable so we can re-use the country id below when we insert it into the dictionary
                var CountryToAdd = new CountryCodeInfo(Convert.ToInt32(CountryToLoad.Attribute("id").Value),
                                                        CountryToLoad.Attribute("shortname").Value,
                                                        CountryToLoad.Attribute("longname").Value,
                                                        CountryToLoad.Attribute("iso2").Value,
                                                        CountryToLoad.Attribute("irs2").Value,
                                                        CountryToLoad.Attribute("iso3char").Value,
                                                        Convert.ToInt32(CountryToLoad.Attribute("iso3digit").Value));

                //add the country value
                ReturnObject.Add(CountryToAdd.CountryID, CountryToAdd);
            }

            //return the dictionary
            return ReturnObject.ToImmutableDictionary();
        }

        #endregion

    }

}
