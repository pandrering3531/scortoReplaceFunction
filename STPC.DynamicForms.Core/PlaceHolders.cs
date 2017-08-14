using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace STPC.DynamicForms.Core
{
    /// <summary>
    /// Static class containing place holder tokens to be used with form and field templates.
    /// </summary>
    public static class PlaceHolders
    {
        /*
         * "Why do you use GUIDs?" I hear you asking.
         * Well, it's because of the very low risk of my using two of the same
         * throughout my code and the very low risk of a developer or user
         * using one in their templates or field definitions. I don't see any
         * downside to using them. I have prefixed them with their intention
         * for debugging purposes.
         */
        private const string FIELDS = "{Fields:1e6e1ef1-8acc-48b7-b1e8-705893b0411c}";
        private const string INPUT = "{Input:2bb514da-20ef-483a-9441-d03d83d0471a}";
        private const string PROMPT = "{Prompt:3cb27d35-c390-440a-9114-838e8ba0acf3}";
        private const string ERROR = "{Error:0c9cb943-e4b1-480e-ba27-bc1cba8ceefe}";
        private const string LITERAL = "{Literal:9a119965-583e-4874-9442-201b0f082fd6}";
        private const string SERIALIZEDFORM = "{SerializedForm:592a069d-b1b2-4489-9fc2-6dc2f5d57ca3}";
        private const string DATASCRIPT = "{DataScript:e535d8d3-1e7b-4f1d-a3a4-2e55e9c3c0c4}";
        private const string FIELDWRAPPERID = "{FieldWrapperId:0ec5e0a1-a01a-4384-9f73-5c06ab2db5d3}";
        private const string LHYPERLINK = "{LHyper:6870D9E3-C7DE-4597-9632-74DF9E94A2D0}";

        /// <summary>
        /// Place holder for fields' rendered HTML.
        /// For use only with the Form.Template property.
        /// </summary>
        public static string Fields
        {
            get
            {
                return Constants.PH_FIELDS;
            }
        }
        /// <summary>
        /// Place holder for an InputField's input elements. 
        /// For use only with the InputField.Template property.
        /// </summary>
        public static string Input
        {
            get
            {
                return Constants.PH_INPUT;
            }
        }
        /// <summary>
        /// Place holder for an InputField's prompt label element. 
        /// For use only with the InputField.Template property.
        /// </summary>
        public static string Prompt
        {
            get
            {
                return Constants.PH_PROMPT;
            }
        }
        /// <summary>
        /// Place holder for an InputField's error label element. 
        /// For use only with the InputField.Template property.
        /// </summary>
        public static string Error
        {
            get
            {
                return Constants.PH_ERROR;
            }
        }
        /// <summary>
        /// Place holder for a Literal field's html. 
        /// For use only with the Literal.Template property.
        /// </summary>
        public static string Literal
        {
            get
            {
                return Constants.PH_LITERAL;
            }
        }
        /// <summary>
        /// Place holder for a Form's serialized state as a hidden html input element. 
        /// For use only with the Form.Template property.
        /// </summary>
        public static string SerializedForm
        {
            get
            {
                return Constants.PH_SERIALIZEDFORM;
            }
        }
        /// <summary>
        /// Place holder for Form's Fields' client-side data represented as JSON in an HTML script element.
        /// For use only with the Form.Template property.
        /// </summary>
        public static string DataScript
        {
            get
            {
                return Constants.PH_DATASCRIPT;
            }
        }
        /// <summary>
        /// Place holder for a Field's wrapping element's ID attribute value.
        /// The wrapping element is given a unique ID for DOM manipulation purposes.
        /// For use only with the Field.Template property.
        /// </summary>
        public static string FieldWrapperId
        {
            get
            {
                return Constants.PH_FIELDWRAPPERID;
            }
        }

        /// <summary>
        /// Place holder for a Literal field's html. 
        /// For use only with the Literal.Template property.
        /// </summary>
        public static string LHyperLink
        {
            get
            {
                return Constants.PH_LHYPERLINK;
            }
        }
        

        internal static StringBuilder RemoveAll(StringBuilder html)
        {
            return html
                .Replace(Constants.PH_FIELDS, string.Empty)
                .Replace(Constants.PH_INPUT, string.Empty)
                .Replace(Constants.PH_PROMPT, string.Empty)
                .Replace(Constants.PH_ERROR, string.Empty)
                .Replace(Constants.PH_LITERAL, string.Empty)
                .Replace(Constants.PH_LHYPERLINK, string.Empty)
                .Replace(Constants.PH_SERIALIZEDFORM, string.Empty)
                .Replace(Constants.PH_DATASCRIPT, string.Empty)
                .Replace(Constants.PH_FIELDWRAPPERID, string.Empty);
        }
    }
}
