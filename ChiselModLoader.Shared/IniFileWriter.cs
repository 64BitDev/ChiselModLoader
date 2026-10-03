using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace ChiselModLoader.Shared.Ini
{
	public sealed class IniFileWriter
	{
		private readonly char[] m_CRLF = new char[] { '\r', '\n' };

		private readonly Dictionary<string, Dictionary<string, string>> m_Sections;

		private readonly Encoding m_Encoding;

		/// <summary>
		/// Creates a new instance of the IniFileWriter class.
		/// </summary>
		/// <param name="encoding">
		/// The text encoding to use when reading the INI file. Defaults to 
		/// UTF-8 encoding if set null or omitted.
		/// </param>
		public IniFileWriter(Encoding encoding = null)
		{
			m_Encoding = encoding ?? Encoding.UTF8;

			m_Sections = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
		}

		/// <summary>
		/// Adds a new entry to the INI file at the specified section, with the
		/// specified key and the specified value.
		/// </summary>
		/// <param name="section">
		/// The name of the section that contains the key.
		/// </param>
		/// <param name="key">
		/// The name of the key that contains the value.
		/// </param>
		/// <param name="value">
		/// The value to store.
		/// </param>
		/// <param name="throwOnCRLF">
		/// If true, throws an ArgumentException if the value contains carriage
		/// return (CR) or line feed (LF) characters. If false, carriage return
		/// (CR) and line feed (LF) characters are replaced with spaces.
		/// </param>
		/// <remarks>
		/// An ArgumentException  is thrown if either the section or the key
		/// contain carriage return (CR) or line feed (LF) characters.
		/// 
		/// If a section and key already exist, subsequent calls with the same
		/// section and key will overwrite the previously stored value.
		/// </remarks>
		public void Add(string section, string key, string value = null, bool throwOnCRLF = false)
		{
			if (section == null)
				throw new ArgumentNullException("section");

			if (section.IndexOfAny(m_CRLF) != -1)
				throw new ArgumentException("Unsupported character in section.");

			if (key == null)
				throw new ArgumentNullException("key");

			if (key.IndexOfAny(m_CRLF) != -1)
				throw new ArgumentException("Unsupported character in key.");

			if (value == null)
				value = string.Empty;

			if (value.IndexOfAny(m_CRLF) != -1)
			{
				if (throwOnCRLF)
					throw new ArgumentException("Unsupported character in value.");

				value = value.Replace('\r', ' ');
				value = value.Replace('\n', ' ');
			}

			if (!m_Sections.ContainsKey(section))
				m_Sections.Add(section, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));

			if (!m_Sections[section].ContainsKey(key))
				m_Sections[section].Add(key, value);
			else
				m_Sections[section][key] = value;
		}

		/// <summary>
		/// Adds a new entry to the INI file at the specified section, with the
		/// specified key and the specified value.
		/// </summary>
		/// <param name="section">
		/// The name of the section that contains the key.
		/// </param>
		/// <param name="key">
		/// The name of the key that contains the value.
		/// </param>
		/// <param name="value">
		/// The value to store. The return value of the object's ToString()
		/// method will be stored.
		/// </param>
		/// <param name="throwOnCRLF">
		/// If true, throws an ArgumentException if the value contains carriage
		/// return (CR) or line feed (LF) characters. If false, carriage return
		/// (CR) and line feed (LF) characters are replaced with spaces.
		/// </param>
		/// <remarks>
		/// An ArgumentException is thrown if either the section or the key
		/// contain carriage return (CR) or line feed (LF) characters.
		/// 
		/// If a section and key already exist, subsequent calls with the same
		/// section and key will overwrite the previously stored value.
		/// </remarks>
		public void Add(string section, string key, object value, bool throwOnCRLF = false)
		{
			Add(section, key, value.ToString(), throwOnCRLF);
		}

		/// <summary>
		/// Saves the INI file to disk.
		/// </summary>
		/// <param name="path">
		/// The complete file path to save the INI file to.
		/// </param>
		/// <exception cref="UnauthorizedAccessException">
		/// Access is denied.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// path is empty. -or- path contains the name of a system device
		/// (com1, com2, and so on).
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// path is null.
		/// </exception>
		/// <exception cref="DirectoryNotFoundException">
		/// The specified path is invalid (for example, it is on an unmapped
		/// drive).
		/// </exception>
		/// <exception cref="IOException">
		/// path includes an incorrect or invalid syntax for file name,
		/// directory name, or volume label syntax.
		/// </exception>
		/// <exception cref="PathTooLongException">
		/// The specified path, file name, or both exceed the system-defined
		/// maximum length.
		/// </exception>
		/// <exception cref="IOException">
		/// The caller does not have the required permission.
		/// </exception>
		public void Save(string path)
		{
			using (StreamWriter sw = new StreamWriter(path, false, m_Encoding))
				foreach (string section in m_Sections.Keys)
				{
					sw.WriteLine(string.Format(CultureInfo.InvariantCulture, "[{0}]", section));

					foreach (string key in m_Sections[section].Keys)
						sw.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0}={1}", key, m_Sections[section][key]));
				}
		}
	}
}
