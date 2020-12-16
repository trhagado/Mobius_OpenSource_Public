using Mobius.ComOps;

using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Threading.Tasks;

//using Microsoft.AspNetCore.Components;
//using Microsoft.AspNetCore.Components.Web;

/// <summary>
/// Div management for Mobius Winforms/DX Controls / Razor Components integation
/// </summary>

namespace Mobius.ComOps
{
  public class CssStyleMx
  {
    public string StyleString { get => _styleString; set => SetStyleString(value); }
    public string OriginalStyleString = null;

    public CssStyleMx()
    {
      return;
    }

    public CssStyleMx(string styleString)
    {
      StyleString = styleString;
      return;
    }

    /// <summary>
    /// Set StyleString
    /// </summary>
    /// <param name="styleString"></param>

    public void SetStyleString(string styleString)
    {
      if (styleString == null) styleString = styleString; // debug;

      if (OriginalStyleString == null && _styleString != null) // save original if not done yet
      {
        OriginalStyleString = _styleString;
      }

      _styleString = styleString;
    }
    string _styleString = null;

    /// <summary>
    /// Get the integer part of a pixel measurement property 
    /// </summary>
    /// <param name="styleString"></param>
    /// <param name="propName"></param>
    /// <returns></returns>

    public int GetIntPxProperty(
      string propName)
    {
      int ipx;
      string s = GetProperty(propName);
      if (Lex.IsUndefined(s)) return -1;

      if (TryParseIntPx(s, out ipx))
        return ipx;

      else return -1;
    }

    /// <summary>
    /// Set an integer pixel measurement property 
    /// </summary>
    /// <param name="propName"></param>
    /// <param name="propValue"></param>

    public void SetIntPxProperty(
      string propName,
      int propValue)
    {
      string s = null;
      if (propValue != NumberEx.NullNumber)
        s = propValue + "px";

      SetProperty(propName, s);
      return;
    }

    /// <summary>
    /// Set a property value
    /// </summary>
    /// <param name="propName"></param>
    /// <param name="propValue"></param>

    public void SetProperty(
      string propName,
      string propValue)
    {
      StyleString = SetProperty(StyleString, propName, propValue);
      return;
    }

    /// <summary>
    /// Set a style property
    /// </summary>
    /// <param name="styleString"></param>
    /// <param name="propName"></param>
    /// <param name="propValue"></param>
    /// <returns>Modified style string</returns>

    public static string SetProperty(
      string styleString,
      string propName,
      string propValue)
    {
      StringBuilder sb = new StringBuilder();
      bool propSeen = false;
      var props = Lex.Split(styleString, ";");
      foreach (string prop in props)
      {
        if (Lex.IsUndefined(prop)) continue;

        var kvp = Lex.Split(prop, ":");
        if (kvp.Length != 2) continue;

        if (Lex.Eq(kvp[0], propName))
        {
          if (String.Equals(propValue, kvp[1])) // setting to current value?
            return styleString; // just return original style string

          if (Lex.IsDefined(propValue))
            sb.Append(propName).Append(":").Append(propValue).Append("; ");

          propSeen = true;
        }

        else // just append if other property
        {
          sb.Append(prop).Append("; ");
        }
      }

      if (!propSeen && Lex.IsDefined(propValue)) // append if not seen yet
        sb.Append(propName).Append(":").Append(propValue).Append("; ");

      string newStyleString = sb.ToString();
      return newStyleString;
    }

    public string GetProperty(
      string propName)
    {
      return GetProperty(StyleString, propName);
    }

    /// <summary>
    /// Get a style property
    /// </summary>
    /// <param name="styleString"></param>
    /// <param name="propName"></param>
    /// <returns>Property value</returns>

    public static string GetProperty(
      string styleString,
      string propName)
    {
      var props = Lex.Split(styleString, ";");
      foreach (string prop in props)
      {
        var values = Lex.Split(prop, ":");

        if (Lex.Eq(values[0], propName))
          return values[1];
      }

      return null;
    }

    /// <summary>
    /// PropertyIsDefined
    /// </summary>
    /// <param name="styleString"></param>
    /// <param name="propName"></param>
    /// <returns></returns>

    public static bool PropertyIsDefined(
      string styleString,
      string propName)
    {
      return Lex.IsDefined(GetProperty(styleString, propName));
    }

    /// <summary>
    /// PropertyIsUndefined
    /// </summary>
    /// <param name="styleString"></param>
    /// <param name="propName"></param>
    /// <returns></returns>

    public static bool PropertyIsUndefined(
      string styleString,
      string propName)
    {
      return Lex.IsUndefined(GetProperty(styleString, propName));
    }

    /// <summary>
    /// Parse an int value with px (pixel) suffix
    /// </summary>
    /// <param name="txt"></param>
    /// <returns></returns>

    public static int ParseIntPx(string s)
    {
      int intVal = -1;

      if (TryParseIntPx(s, out intVal))
        return intVal;

      else throw new InvalidDataException("Invalid pixel int: " + s);
    }

    /// <summary>
    /// Parse an int value with px (pixel) suffix
    /// </summary>
    /// <param name="s"></param>
    /// <param name="result"></param>
    /// <returns></returns>

    public static bool TryParseIntPx(string s, out Int32 result)
    {
      s = Lex.Trim(s);
      s = Lex.RemoveAllQuotes(s);
      s = Lex.Trim(s);

      if (Lex.EndsWith(s, "px"))
        s = s.Substring(0, s.Length - 2);

      if (int.TryParse(s, out result))
        return true;

      else return false;
    }
  }


}

