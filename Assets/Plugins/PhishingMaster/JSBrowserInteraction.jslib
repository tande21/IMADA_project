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
mergeInto(LibraryManager.library, {

  BrowserAlertString: function (str) {
    window.alert(Pointer_stringify(str));
  },
  
  GetChallengeCodeFromURL: function () {
    var url_string = window.location.href
    var url = new URL(url_string);
    var returnStr = url.searchParams.get("c");
	if(returnStr == null) {
		returnStr = "none";
	}
    var bufferSize = lengthBytesUTF8(returnStr) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(returnStr, buffer, bufferSize);
    return buffer;
  },
  
  JSGetBrowserLanguage: function () {
    var lang = navigator.language || navigator.userLanguage; 
    var returnStr = lang;
    var bufferSize = lengthBytesUTF8(returnStr) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(returnStr, buffer, bufferSize);
    return buffer;
  },
  
  JSCopyStringToClipboardOld: function (str) {
    var challengeCodeText = document.getElementById("challengeCodeText");
    challengeCodeText.value = Pointer_stringify(str);
    challengeCodeText.select();
    challengeCodeText.setSelectionRange(0, 99999)
    document.execCommand("copy");
    alert("Copied the text: " + challengeCodeText.value);
  },
  
  JSDisplayChallengeCode: function (str, score) {
	UnityDisplayChallengeCode(Pointer_stringify(str), score);
  },
  
  JSPlayerNameSubmitted: function (str, score) {
	UnityPlayerNameSubmitted(Pointer_stringify(str), score);
  },
  
  JSGetURL: function () {
    var returnStr = window.location.protocol + "//" + window.location.hostname + window.location.pathname;
	var bufferSize = lengthBytesUTF8(returnStr) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(returnStr, buffer, bufferSize);
    return buffer;
  },

});
