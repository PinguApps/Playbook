using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;

namespace Playbook.Samples;

// snippet:caption
public static class FieldCaption
{
    public static string For<T>(Expression<Func<T>> field)
    {
        var member = ((MemberExpression)field.Body).Member;
        return member.GetCustomAttribute<DisplayAttribute>()?.GetName()
            ?? member.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName
            ?? member.Name;
    }
}
// endsnippet:caption
