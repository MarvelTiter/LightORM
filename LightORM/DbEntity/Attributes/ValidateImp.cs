using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MDbEntity.Attributes {
    public class ValidateImp<T> : IValidate<T> {
        readonly List<string> outMsg;
        public List<string> ValidateMsg => outMsg;
        public ValidateImp() {
            outMsg = new List<string>();
        }
        public bool Validate(T entity) {
#if NET40
            
#else
            //if (entity == null)
            //    return false;
            //string outMsgTemplate = "{0}:{1}";
            //var props = typeof(T).GetProperties();
            //var type = typeof(IValidateAttribute);

            //foreach (PropertyInfo propInfo in props) {
            //    var name = propInfo.GetCustomAttribute<DisplayAttribute>();
            //    var displayName = name == null ? propInfo.Name : name.DisplayName;
            //    var propValue = propInfo.GetValue(entity);

            //    foreach (CustomAttributeData cusAttr in propInfo.CustomAttributes) {
            //        var atrType = cusAttr.AttributeType;
            //        if (type.IsAssignableFrom(atrType) && !atrType.IsAbstract) {
            //            var vat = propInfo.GetCustomAttribute(atrType);
            //            if (vat is IValidateAttribute) {
            //                var va = vat as IValidateAttribute;
            //                var match = va?.Check(propValue);
            //                if (!match.HasValue || !match.Value) {
            //                    outMsg.Add(string.Format(outMsgTemplate, displayName, va.ErrorMsg()));
            //                }
            //            }
            //        }

            //    }
            //}
#endif
            return outMsg.Count == 0;
        }
    }
}
