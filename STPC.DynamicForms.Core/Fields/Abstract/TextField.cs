using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace STPC.DynamicForms.Core.Fields
{
	/// <summary>
	/// Represents an html input field that will accept a text response from the user.
	/// </summary>
	[Serializable]
	public abstract class TextField : InputField
	{
		//private string _regexMessage = "Invalid";
		private string _regexMessage = Constants.TF_regexMessage;

		/// <summary>
		/// A regular expression that will be applied to the user's text respone for validation.
		/// </summary>
		public string RegularExpression { get; set; }
		/// <summary>
		/// The error message that is displayed to the user when their response does no match the regular expression.
		/// </summary>
		public string RegexMessage
		{
			get
			{
				return _regexMessage;
			}
			set
			{
				_regexMessage = value;
			}
		}
		private string _value;
		public string Value
		{
			get
			{
				return _value ?? "";
			}
			set
			{
				_value = value;
			}
		}
		public string Index;
		public override string Response
		{
			get { return Value.Trim(); }
		}
		public override bool Validate()
		{
			ClearError();

			if (string.IsNullOrEmpty(Response))
			{
				if (Required)
				{
					// invalid: is required and no response has been given
					Error = RequiredMessage;
				}
			}
			else
			{
				if (!string.IsNullOrEmpty(RegularExpression))
				{
					var regex = new Regex(RegularExpression);
					if (!regex.IsMatch(Value))
					{
						// invalid: has regex and response doesn't match
						Error = RegexMessage;
					}
				}
			}

			FireValidated();
			return ErrorIsClear;
		}
		public string EventParent { get; set; }

		public bool isMathExpresion;
		public string mathExpresion;

		public int? idStrategy;
		public string Style { get; set; }
		public bool IsRequeried { get; set; }

		public bool IsTriguerEventChange { get; set; }

		public string ValueWhenHideControl { get; set; }

		public string IdControlToHide { get; set; }

		public string eventName { get; set; }

		public bool isEmail { get; set; }


		
	}
}
