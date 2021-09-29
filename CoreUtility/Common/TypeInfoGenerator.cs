using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreUtility
{
    public enum DataType
    {
        Unknown,
        Array,
        Boolean,
        Collection,
        DateTime,
        Double,
        String,
        Int,
        Long,
        ComplexObject
    }

    public class TypeInfo
    {
        public string FieldName { get; set; }

        public DataType DataType { get; set; }

        public string UnderlyingType { get; set; }

        public IList<TypeInfo> Members { get; set; }

        public override string ToString()
        {
            return $"{FieldName} - {DataType}";
        }

        public TypeInfo(string fieldName, DataType dataType)
        {
            FieldName = fieldName;
            DataType = dataType;
        }
    }

    public class TypeInfoGenerator
    {
        public IEnumerable<TypeInfo> GetTypeInfo(Type type, string name)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            if (name is null)
                throw new ArgumentNullException(nameof(name));

            var types = new List<TypeInfo>();
            ICollection<Type> capturedTypeList = new List<Type>();
            Queue<Type> tobeProcessed = new Queue<Type>();
            capturedTypeList.Add(type);
            tobeProcessed.Enqueue(type);
            while (tobeProcessed.Any())
            {
                var nestedType = tobeProcessed.Dequeue();
                types.Add(Generate(nestedType, nestedType.Name, tobeProcessed, capturedTypeList));
            }
            return types;
        }

        private TypeInfo Generate(Type type, string name, Queue<Type> tobeProcessed, ICollection<Type> capturedTypeList)
        {
            var typeInfo = new TypeInfo(name, GetDataType(type)) {
                Members = new List<TypeInfo>(),
                UnderlyingType = type.Name
            };
            foreach (var prop in type.GetProperties())
            {
                var nestedType = new TypeInfo(prop.Name, GetDataType(prop.PropertyType));
                typeInfo.Members.Add(nestedType);

                if (nestedType.DataType == DataType.Collection || nestedType.DataType == DataType.ComplexObject)
                {
                    var genericType = nestedType.DataType == DataType.Collection ? prop.PropertyType.GenericTypeArguments[0] : prop.PropertyType;
                    nestedType.UnderlyingType = genericType.Name;
                    if (!capturedTypeList.Any(t => t == genericType))
                    {
                        capturedTypeList.Add(genericType);
                        tobeProcessed.Enqueue(genericType);
                    }
                }
                else
                {
                    nestedType.UnderlyingType = prop.PropertyType.Name;
                }
            }

            return typeInfo;
        }


        private DataType GetDataType(Type type)
        {
            if (type.IsArray)
                return DataType.Array;
            if (type == typeof(int))
                return DataType.Int;
            if (type == typeof(string))
                return DataType.String;
            if (type == typeof(bool))
                return DataType.Boolean;
            if (type == typeof(DateTime))
                return DataType.DateTime;
            if (type == typeof(long))
                return DataType.Long;
            if (type == typeof(double))
                return DataType.Double;
            if (type.IsGenericType &&
                ((type.GetGenericTypeDefinition().Equals(typeof(ICollection<>))) ||
                (type.GetGenericTypeDefinition().Equals(typeof(IEnumerable<>))) ||
                (type.GetGenericTypeDefinition().Equals(typeof(IList<>)))))
                return DataType.Collection;
            if (type.IsClass && !type.FullName.StartsWith("System."))
                return DataType.ComplexObject;
            return DataType.Unknown;
        }
    }
    
    
    //Usage
      public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IList<Order> Orders { get; set; }
        public Boolean IsVipMember { get; set; }
    }

    public class Order
    {
        public Int64 Id { get; set; }

        public DateTime OrderDate{ get; set; }

        public double OrderTotal { get; set; }

        public IEnumerable<Product> Items { get; set; }

        public Address ShippingAddress { get; set; }

        public Address BillingAddress { get; set; }
    }

    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
    }

    public class Address
    {
        public int Id { get; set; }
        public string Street1 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
    }
      public static void Main()
        {
             new TypeInfoGenerator().GetTypeInfo(typeof(Customer), nameof(Customer));
           
          }
    }
}


