using System;
using System.Collections.Generic;

namespace Yardarm.Names
{
    public class NamingContext
    {
        private readonly Dictionary<string, int> _names = new Dictionary<string, int>();

        /// <summary>
        /// Adds a name to the naming context, and returns the actual name to use.
        /// </summary>
        /// <param name="name">Name to add.</param>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is null.</exception>
        /// <returns>The name to use. May be the name with a number appended to prevent conflicts.</returns>
        public string RegisterName(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (!_names.TryGetValue(name, out int count))
            {
                _names[name] = 0;
                return name;
            }

            _names[name] = ++count;

            return name + count;
        }
    }
}
