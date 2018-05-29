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
using System.Collections.Generic;

public class Member
{
    private int memberNumber;
    private int memberChannel;
    private String memberType;
    private String memberAlias;
 
    public Member()
    {
        // Leave fields empty.
    }

    public Member(int memberNumber, int memberChannel, String memberType, String memberAlias)
    {
        this.memberNumber = memberNumber;
        this.memberChannel = memberChannel;
        this.memberType = memberType;
        this.memberAlias = memberAlias;
    }

    public int MemberNumber
    {
        get
        {
            return memberNumber;
        }
        set
        {
            memberNumber = value;
        }
    }

    public int MemberChannel
    {
        get
        {
            return memberChannel;
        }
        set
        {
            memberChannel = value;
        }
    }

    public String MemberType
    {
        get
        {
            return memberType;
        }
        set
        {
            memberType = value;
        }
    }

    public String MemberAlias
    {
        get
        {
            return memberAlias;
        }
        set
        {
            memberAlias = value;
        }
    }
}
