/*
 * 
 *  Copyright(c) 2018 Jean-Michel Cohen
 *  
 *  Permission is hereby granted, free of charge, to any person obtaining a copy
 *  of this software and associated documentation files (the "Software"), to deal
 *  in the Software without restriction, including without limitation the rights
 *  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 *  copies of the Software, and to permit persons to whom the Software is
 *  furnished to do so, subject to the following conditions:
 *
 *  The above copyright notice and this permission notice shall be included in
 *  all copies or substantial portions of the Software.
 *
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 *  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 *  THE SOFTWARE.
*/
using System;

public class Contact
{
    private String number;
    private String callType;
    private String contactAlias;
    private String city;
    private String province;
    private String country;
    private String callID;

    public Contact() {
        // Leave fields empty.
    }

    public Contact(
        String number, String callType, String contactAlias, String city, String province, String country, String callID
    )
    {
        this.number = number;
        this.callType = callType;
        this.contactAlias = contactAlias;
        this.city = city;
        this.province = province;
        this.country = country;
        this.callID = callID;
    }

    public String Number
    {
        get
        {
            return number;
        }
        set
        {
            number = value;
        }
    }

    public String CallType
    {
        get
        {
            return callType;
        }
        set
        {
            callType = value;
        }
    }
    public String ContactAlias
    {
        get
        {
            return contactAlias;
        }
        set
        {
            contactAlias = value;
        }
    }
    public String City
    {
        get
        {
            return city;
        }
        set
        {
            city = value;
        }
    }
    public String Province
    {
        get
        {
            return province;
        }
        set
        {
            province = value;
        }
    }
    public String Country
    {
        get
        {
            return country;
        }
        set
        {
            country = value;
        }
    }
    public String CallID
    {
        get
        {
            return callID;
        }
        set
        {
            callID = value;
        }
    }
}
