using System;
using System.Collections.Generic;
using System.Text;

namespace LightORM;

[AttributeUsage(AttributeTargets.Property)]
public class IgnoreAttribute : Attribute
{
}
