using System;
using System.Collections;
using System.Reflection;

namespace MapClassesWithSameStructure
{
    class Program
    {
        /// <summary>
        /// Maps two classes with the same structure, regardless of the property namings
        /// </summary>
        /// <typeparam name="Source"> The source class which has the data </typeparam>
        /// <typeparam name="Destination"> The destination class that will have the data loaded into it </typeparam>
        /// <param name="source"> An object of type Source. </param>
        /// <param name="destination"> An object of type destination </param>
        /// <returns> Destination which has properties with equal values to source </returns>
        public static Destination MapClasses<Source, Destination>(Source source, Destination destination)
        {
            // GetType the type of source
            Type type = source.GetType();
            // If class
            if (type.IsClass && !type.IsGenericType && source is not IList && !type.Equals(typeof(string)) && !type.IsEnum && !type.Equals(typeof(decimal)))
            {
                // Get its properties
                PropertyInfo[] sourceProperties = source.GetType().GetProperties();
                PropertyInfo[] destinationProperties = destination.GetType().GetProperties();

                // For each property
                for (int i = 0; i < sourceProperties.Length; i++)
                {
                    // If primitive
                    if (sourceProperties[i].PropertyType.IsPrimitive || sourceProperties[i].PropertyType.Equals(typeof(string)))
                    {
                        // Set the value of destination[i] to source[i]
                        destinationProperties[i].SetValue(destination, sourceProperties[i].GetValue(source));
                    }
                    // If not, it is a class or a list
                    else
                    {
                        // If it is null
                        if (destinationProperties[i].GetValue(destination) == null)
                        {
                            // Insantiate it
                            destinationProperties[i].SetValue(destination, Activator.CreateInstance(destinationProperties[i].PropertyType));
                        }
                        // Map destination[i] to source[i] and get its value
                        var c = MapClasses(sourceProperties[i].GetValue(source), destinationProperties[i].GetValue(destination));

                        // Set the mapped value
                        destinationProperties[i].SetValue(destination, c);
                    }
                }
            }
            // If list
            else if (type.IsGenericType && source is IList)
            {
                // Cast source and destination to list
                var sourceList = source as IList;
                var destinationList = destination as IList;

                // Get the lists type
                Type listType = type.GenericTypeArguments[0];

                // If list of primitives
                if (listType.IsPrimitive || listType.Equals(typeof(string)))
                {
                    // For each member
                    for (int i = 0; i < sourceList.Count; i++)
                    {
                        // Add source value to destination
                        destinationList.Add(sourceList[i]);
                    }
                }
                // Else, it is a list of class
                else
                {
                    // For each member
                    for (int i = 0; i < sourceList.Count; i++)
                    {
                        // Map it by creating a new instance for destination
                        destinationList.Add(MapClasses(sourceList[i], Activator.CreateInstance(destinationList.GetType().GenericTypeArguments[0])));
                    }
                }
                destination = (Destination)destinationList;
            }

            // Return destination that is made equal to source
            return destination;
        }
    }
}
