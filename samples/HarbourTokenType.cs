using System.Globalization;
using System.Reflection;

namespace Playbook.Samples;

// snippet:custom-base
// A metadata-only diagnostic token: not a runtime-loadable CLR type.
// Its minimal reflection model has no members and can never represent Nullable<T>.
public class HarbourTokenType : Type
{
    public override string Name => "Token";
    public override string Namespace => "Harbour.Diagnostics";
    public override string FullName => Namespace + "." + Name;
    public override string AssemblyQualifiedName => FullName + ", " + Assembly.FullName;
    public override Assembly Assembly => typeof(HarbourTokenType).Assembly;
    public override Module Module => Assembly.ManifestModule;
    public override Type UnderlyingSystemType => this;
    public override Type BaseType => typeof(object);
#pragma warning disable CA1720 // The member name is required by System.Type.
    public override Guid GUID => Guid.Empty;
#pragma warning restore CA1720

    protected override TypeAttributes GetAttributeFlagsImpl() => TypeAttributes.Public | TypeAttributes.Class;
    protected override bool IsArrayImpl() => false;
    protected override bool IsByRefImpl() => false;
    protected override bool IsPointerImpl() => false;
    protected override bool IsPrimitiveImpl() => false;
    protected override bool IsCOMObjectImpl() => false;
    protected override bool HasElementTypeImpl() => false;
    public override Type? GetElementType() => null;
    public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr) => [];
    public override MethodInfo[] GetMethods(BindingFlags bindingAttr) => [];
    public override FieldInfo[] GetFields(BindingFlags bindingAttr) => [];
    public override EventInfo[] GetEvents(BindingFlags bindingAttr) => [];
    public override PropertyInfo[] GetProperties(BindingFlags bindingAttr) => [];
    public override Type[] GetInterfaces() => [];
    public override Type[] GetNestedTypes(BindingFlags bindingAttr) => [];
    public override MemberInfo[] GetMembers(BindingFlags bindingAttr) => [];
    public override FieldInfo? GetField(string name, BindingFlags bindingAttr) => null;
    public override EventInfo? GetEvent(string name, BindingFlags bindingAttr) => null;
    public override Type? GetInterface(string name, bool ignoreCase) => null;
    public override Type? GetNestedType(string name, BindingFlags bindingAttr) => null;
    public override object[] GetCustomAttributes(bool inherit) => [];
    public override object[] GetCustomAttributes(Type attributeType, bool inherit) => [];
    public override bool IsDefined(Type attributeType, bool inherit) => false;

    protected override ConstructorInfo? GetConstructorImpl(BindingFlags bindingAttr, Binder? binder,
        CallingConventions callConvention, Type[] types, ParameterModifier[]? modifiers) => null;
    protected override MethodInfo? GetMethodImpl(string name, BindingFlags bindingAttr, Binder? binder,
        CallingConventions callConvention, Type[]? types, ParameterModifier[]? modifiers) => null;
    protected override PropertyInfo? GetPropertyImpl(string name, BindingFlags bindingAttr, Binder? binder,
        Type? returnType, Type[]? types, ParameterModifier[]? modifiers) => null;
    public override object? InvokeMember(string name, BindingFlags invokeAttr, Binder? binder, object? target,
        object?[]? args, ParameterModifier[]? modifiers, CultureInfo? culture, string[]? namedParameters) =>
        throw new NotSupportedException("A metadata-only harbour token cannot execute members.");
}
// endsnippet:custom-base
