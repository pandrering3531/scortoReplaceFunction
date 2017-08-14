using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace STPC.DynamicForms.Core
{
	public static class Constants
	{
		//MagicString
		public const string DF_DynamicSerializedForm = "STPC_DynamicSerializedForm";

		//Form
		public const string FR_formWrapper = "div";
		public const string FR_formWrapperClass = "STPC_DFr";
		public const string FR_fieldPrefix = "STPC_DFi_";
		public const string FR_jsVarName = "STPC_DFiData";

		//Field
		public const string F_fieldWrapper = "div";
		public const string F_fieldWrapperClass = "STPC_FieldWrapper";

		//Input Field
		public const string IF_requiredMessage = "Requerido";
		public const string IF_promptClass = "STPC_DynamicFieldPrompt";
		public const string IF_errorClass = "STPC_DynamicFieldError";
		public const string IF_CSSClass = "MvcFieldWrapper";

		//Text Field
		public const string TF_regexMessage = "Formato invalido";
		public const string TF_MAX_TEXTO = "512";
		public const string TF_MAX_NUMERO = "10";
		//public const string TF_MAX_FECHA = DateTime.MaxValue.ToString();
		public const string TF_MAX_CORREO = "512";
		public const string TF_MAX_LISTA_UNICA = "512";
		public const string TF_MAX_LISTA_MULTIPLE = "512";
		public const string TF_MAX_TEXTAREA = "4000";
		public const string TF_MAX_LISTA_SELECT = "512";
		public const string TF_MAX_CHECK = "1";
		//public const string TF_MAX_FILE = "2048"; // valor en KBytes
		public const string TF_MAX_LITERAL = "512";
		public const string TF_MAX_LINK = "512";
		public const string TF_MAX_MONEDA = "19";
		public const string TF_MAX_DECIMAL = "38";

		//OrientableField
		public const string OF_inputLabelClass = "STPC_DynamicListFieldInputLabel";
		public const string OF_verticalClass = "STPC_DynamicVertical";
		public const string OF_horizontalClass = "STPC_DynamicHorizontal";
		public const string OF_listClass = "STPC_DynamicOrientableList";

		//Checkbox
		public const string CB_promptClass = "STPC_DynamicCheckboxPrompt";
		public const string CB_checkedValue = "Yes";
		public const string CB_uncheckedValue = "No";
		public const string CB_CSSClass = "MvcFieldWrapper";

		//PlaceholderPrefixes
		public const string PH_FIELDS = "{Fields:1e6e1ef1-8acc-48b7-b1e8-705893b0411c}";
		public const string PH_INPUT = "{Input:2bb514da-20ef-483a-9441-d03d83d0471a}";
		public const string PH_PROMPT = "{Prompt:3cb27d35-c390-440a-9114-838e8ba0acf3}";
		public const string PH_ERROR = "{Error:0c9cb943-e4b1-480e-ba27-bc1cba8ceefe}";
		public const string PH_LITERAL = "{Literal:9a119965-583e-4874-9442-201b0f082fd6}";
		public const string PH_SERIALIZEDFORM = "{SerializedForm:592a069d-b1b2-4489-9fc2-6dc2f5d57ca3}";
		public const string PH_DATASCRIPT = "{DataScript:e535d8d3-1e7b-4f1d-a3a4-2e55e9c3c0c4}";
		public const string PH_FIELDWRAPPERID = "{FieldWrapperId:0ec5e0a1-a01a-4384-9f73-5c06ab2db5d3}";
		public const string PH_LHYPERLINK = "{LHyper:6870D9E3-C7DE-4597-9632-74DF9E94A2D0}";

		//RegexPatterns
		public const string RP_HTMLID = @"^[a-zA-Z][-_\da-zA-Z]*$";
		public const string RP_HTMLINPUTNAME = @"^[a-zA-Z][-_:.\da-zA-Z]*$";
		public const string RP_EMAILADDRESS = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}$";

		//QueryBuilder

		public const string WHERE = " Where ";
		public const string FROM = " From ";
		public const string SELECT = " Select ";
		public const string SELECT_ = " Select * ";
		public const string JOIN = " Inner join ";
		public const string AND = " AND ";
		public const string REQUEST_TABLE_NAME = "Request";
		public const string PREFIX_DYNAMIC_FIELD = "F_";
		public const string PREFIX_DYNAMIC_TABLE = "TBL_";
		public const string ON = " On ";
		public const string REQUEST_KEY = "RequestId";
		public const string REQUEST_ID_NAME_DINAMYC_TABLE = "F_RequestId";
		public const string REQUEST_TYPE_TABLE = "RequestId";
		public const string PAGE_FLOW_ID = "PageFlowId";
		public const string UPDATED = "Updated";
		public const string UPDATE_BY = "UpdatedBy";
		public const string CREATE_BY = "CreatedBy";
		public const string INSERT = "Insert Into";
		public const string ORDER_BY = "Order by";
		public const string DESC = "Desc";
		public const string TABLE_HIERARCHY = " Hierarchy ";
		public const string TABLE_USER = " [User] ";
		public const string PREFIX_FIELD_REQUES_TABLE = "F_";
		public const string NAME_FIELD_ID_DEFAULT_ATTR_HIERARCHY = "NodeId";

		public const string ERROR_USER_PASSWORD_INCORRECT = "Nombre de usuario o contraseña no válido.";
		public const string ERROR_PASSWORD_EXPIRED = "La contraseña ha expirado.";
		public const string ERROR_QUESTION_NOT_CONFIGURED = "El usuario no tiene configuradas las preguntas de seguridad, favor comunicarse con la mesa de ayuda.";

		public const string ERROR_QUESTION_NOT_CONFIGURED_BD = "No se pueden validar las preguntas, favor comunicarse con la mesa de ayuda.";



		public const string ERROR_THERE_IS_NOT_USER = "Por favor comunicarse con soporte para reiniciar su contraseña";
		public const string ERROR_AUTENTICATION_FAILED = "Nombre de usuario o contraseña no válido";
		public const string ERROR_ANSWER_FAILED = "Respuestas incorrectas.";
		public const string ERROR_CONFIG_RESET_QUESTION_NUMBRER = "El parametro ResetQuestionsPoolNumber es superior a la cantidad de preguntas configuradas.";
		public const string ERROR_NO_USER_LOGGED = "Sessión cerrada o usuario no logeado";
		public const string ERROR_MAX_ATTEMPT_ALLOWED = "Ha superado el número máximo de intentos, Ingrese el codigo de seguridad";

		public const string ERROR_MIN_REQUIRED_PASSWORD_LENGTH = "Número de caracteres inferior al permitido";

		public const string ERROR_REQUIRED_NON_ALPHANUMERIC_CHAR = "la contraseña requiere mínimo {0} caracteres NO alfanuméricos";

		public const string ERROR_REQUIRED_NUMERIC_CHAR = "la contraseña requiere mínimo {0} caracteres numéricos";

		public const string ERROR_REQUIRED_ALPHA_CHAR = "la contraseña requiere mínimo {0} caracteres alfabéticos";

		public const string ERROR_REQUIRED_UPPER_CHAR = "la contraseña requiere mínimo {0} caracteres Mayúsculas";

		public const string ERROR_REQUIRED_LOWER_CHAR = "la contraseña requiere mínimo {0} caracteres Minúsculas";

		public const string ERROR_PASSWORD_FORMAT = "Formato de contraseña incorrecto";

        public const string ERROR_USER_LOCKED_OUT = "El usuario se encuentra bloqueado";

		public const string attemptCount = "Sessión cerrada o usuario no logeado";
		public const string ATTEMPT_COUNT_ERROR = "00001";

		public const string ROLE_NOT_ALOWED = "Acceso denegado";
		public const string ANOTHER_SESSION_ACTIVE = "El usuario ya tiene una sesión abierta";




	}
}
