using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace ASU_U_Operator.Configuration
{
    public abstract class CfgSectionBase
    {
        public List<ValidationResult> ValidateErrors = new List<ValidationResult>();

        public List<string> ErrorMessages
        {
            get
            {
                return ValidateErrors.Select(item => item.ErrorMessage).ToList();
            }
        }

        public virtual bool Validate()
        {
            var context = new ValidationContext(this, serviceProvider: null, items: null);
            if (ValidateErrors == null)
            {
                ValidateErrors = new List<ValidationResult>();
            }
            var res = Validator.TryValidateObject(
                this, context, ValidateErrors,
                validateAllProperties: true
            );
            return res;
        }
           
        public virtual bool ValidateChild(CfgSectionBase child)
        {
            if (child != null)
            {
                var res = child.Validate();
                ValidateErrors.AddRange(child.ValidateErrors);
                return res;
            }
            return true;
        }

    }
}
