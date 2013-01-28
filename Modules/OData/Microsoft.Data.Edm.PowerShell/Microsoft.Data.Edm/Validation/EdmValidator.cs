using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;
using Microsoft.Data.Edm.Validation.Internal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Data.Edm.Validation
{
	internal static class EdmValidator
	{
		public static bool Validate(this IEdmModel root, out IEnumerable<EdmError> errors)
		{
			IEdmModel edmModel = root;
			Version edmVersion = root.GetEdmVersion();
			Version edmVersionLatest = edmVersion;
			if (edmVersion == null)
			{
				edmVersionLatest = EdmConstants.EdmVersionLatest;
			}
			return edmModel.Validate(edmVersionLatest, out errors);
		}

		public static bool Validate(this IEdmModel root, Version version, out IEnumerable<EdmError> errors)
		{
			return root.Validate(ValidationRuleSet.GetEdmModelRuleSet(version), out errors);
		}

		public static bool Validate(this IEdmModel root, ValidationRuleSet ruleSet, out IEnumerable<EdmError> errors)
		{
			EdmUtil.CheckArgumentNull<IEdmModel>(root, "root");
			EdmUtil.CheckArgumentNull<ValidationRuleSet>(ruleSet, "ruleSet");
			errors = InterfaceValidator.ValidateModelStructureAndSemantics(root, ruleSet);
			return errors.FirstOrDefault<EdmError>() == null;
		}
	}
}