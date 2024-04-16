/*
MIT License

Copyright (c) 2020 Tobias Länge and Philipp Matheis

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Basic filter for user input.
/// </summary>
public static class WordFilter
{
    static List<string> wordList = new List<string>() {
    "arschfotze",
    "arschgeige",
    "arschgesicht",
    "arschloch",
    "fotze",
    "fick",
    "muschi",
    "hurensohn",
    "schlampe",
    "luder",
    "stricher",
    "muterfiker",
    "mutterficker",
    "onanieren",
    "schlampe",
    "schwanz",
    "schwanzlutscher",
    "schwuchtel",
    "schwul",
    "schwuler",
    "fick",
    "ficken",
    "jude",
    "nazi",
    "nsdap",
    "wichser",
    "wixer",
    "wixen",
    "arschloch",
    "arsch",
    "schwanz",
    "hure",
    "schlampe",
    "titten",
    "hitler",
    "opfer",
    "anal",
    "anus",
    "bitch",
    "boob",
    "cock",
    "cum",
    "cunt",
    "dick",
    "fuck",
    "handjob",
    "jizz",
    "nigger",
    "penis",
    };
    public static string filterInput(string input)
    {
        string censoredWord = input;
        foreach (string swearWord in wordList)
        {
            censoredWord = censoredWord.Replace(swearWord, new string('*', swearWord.Length));
        }
        return censoredWord;
    }
}
