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

public class Channel
{
    private String channelNumber;
    private String channelType;
    private String channelAlias;
    private String rxFrequency;
    private String txFrequency;
    private String txPower;
    private String tot;
    private String vox;
    private String voxLevel;
    private String scanAdd;

    private String channelWorkAlone;
    private String defaultTalkAround;
    private String bandwidth;
    private String decQtDqt;
    private String encQtDqt;
    private String txAuthorityA;
    private String relay;
    private String workMode;
    private String slot;
    private String idSetting;

    private String colorCode;
    private String encryption;
    private String encryptionType;
    private String encryptionKey;
    private String promiscuous;
    private String txAuthorityD;
    private String killCode;
    private String wakeUpCode;
    private String contacts;
    private String rxGroupsList;

    private List<String> selected;
    private String gps;
    private String sendGpsInfo;
    private String receiveGpsInfo;
    private String gpsTimingReport;
    private String gpsTimingReportTxContacts;
    private String hidden;

    public Channel()
    {
        // Leave fields empty.
    }

    public Channel(
        String channelNumber, String channelType, String channelAlias, String rxFrequency, 
        String txFrequency, String txPower, String tot, String vox, String voxLevel, String scanAdd, 
        String channelWorkAlone, String defaultTalkAround, String bandwidth, String decQtDqt, 
        String encQtDqt, String txAuthorityA, String relay, String workMode, String slot, 
        String idSetting, String colorCode, String encryption, String encryptionType, 
        String encryptionKey, String promiscuous, String txAuthorityD, String killCode, 
        String wakeUpCode, String contacts, String rxGroupsList, List<String> selected, 
        String gps, String sendGpsInfo, String receiveGpsInfo, String gpsTimingReport, String gpsTimingReportTxContacts,
        String hidden
        )
    {
        this.channelNumber = channelNumber;
        this.channelType = channelType;
        this.channelAlias = channelAlias;
        this.rxFrequency = rxFrequency;
        this.txFrequency = txFrequency;
        this.txPower = txPower;
        this.tot = tot;
        this.vox = vox;
        this.voxLevel = voxLevel;
        this.scanAdd = scanAdd;
        this.channelWorkAlone = channelWorkAlone;
        this.defaultTalkAround = defaultTalkAround;
        this.bandwidth = bandwidth;
        this.decQtDqt = decQtDqt;
        this.encQtDqt = encQtDqt;        this.txPower = txPower;
        this.txAuthorityA = txAuthorityA;
        this.relay = relay;
        this.workMode = workMode;
        this.slot = slot;
        this.idSetting = idSetting;
        this.colorCode = colorCode;
        this.encryption = encryption;
        this.encryptionType = encryptionType;
        this.encryptionKey = encryptionKey;
        this.promiscuous = promiscuous;
        this.txAuthorityD = txAuthorityD;
        this.killCode = killCode;
        this.wakeUpCode = wakeUpCode;
        this.contacts = contacts;
        this.rxGroupsList = rxGroupsList;
        this.selected = selected;
        this.gps = gps;
        this.sendGpsInfo = sendGpsInfo;
        this.receiveGpsInfo = receiveGpsInfo;
        this.gpsTimingReport = gpsTimingReport;
        this.gpsTimingReportTxContacts = gpsTimingReportTxContacts;
        
        this.hidden = hidden;
    }

    public String ChannelNumber
    {
        get
        {
            return channelNumber;
        }
        set
        {
            channelNumber = value;
        }
    }

    public String RxFrequency
    {
        get
        {
            return rxFrequency;
        }
        set
        {
            rxFrequency = value;
        }
    }
    public String TxFrequency
    {
        get
        {
            return txFrequency;
        }
        set
        {
            txFrequency = value;
        }
    }

    public String ChannelType
    {
        get
        {
            return channelType;
        }
        set
        {
            channelType = value;
        }
    }

    public String DecQtDqt
    {
        get
        {
            return decQtDqt;
        }
        set
        {
            decQtDqt = value;
        }
    }

    public String EncQtDqt
    {
        get
        {
            return encQtDqt;
        }
        set
        {
            encQtDqt = value;
        }
    }

    public String TxPower
    {
        get
        {
            return txPower;
        }
        set
        {
            txPower = value;
        }
    }

    public String ChannelAlias
    {
        get
        {
            return channelAlias;
        }
        set
        {
            channelAlias = value;
        }
    }
    public String ScanAdd
    {
        get
        {
            return scanAdd;
        }
        set
        {
            scanAdd = value;
        }
    }

    public String Bandwidth
    {
        get
        {
            return bandwidth;
        }
        set
        {
            bandwidth = value;
        }
    }
    public String More
    {
        get
        {
            return "...";
        }
        set
        {
        }
    }

    public String Hidden
    {
        get
        {
            return hidden;
        }
        set
        {
            hidden = value;
        }
    }

    public List<String> Selected
    {
        get
        {
            return selected;
        }
        set
        {
            selected = value;
        }
    }

    public String VoxLevel
    {
        get
        {
            return voxLevel;
        }
        set
        {
            voxLevel = value;
        }
    }

    public String Tot
    {
        get
        {
            return tot;
        }
        set
        {
            tot = value;
        }
    }

    public String Vox
    {
        get
        {
            return vox;
        }
        set
        {
            vox = value;
        }
    }

    public String ChannelWorkAlone
    {
        get
        {
            return channelWorkAlone;
        }
        set
        {
            channelWorkAlone = value;
        }
    }

    public String DefaultTalkAround
    {
        get
        {
            return defaultTalkAround;
        }
        set
        {
            defaultTalkAround = value;
        }
    }

    public String TxAuthorityA
    {
        get
        {
            return txAuthorityA;
        }
        set
        {
            txAuthorityA = value;
        }
    }

    public String Relay
    {
        get
        {
            return relay;
        }
        set
        {
            relay = value;
        }
    }

    public String WorkMode
    {
        get
        {
            return workMode;
        }
        set
        {
            workMode = value;
        }
    }

    public String Slot
    {
        get
        {
            return slot;
        }
        set
        {
            slot = value;
        }
    }

    public String IdSetting
    {
        get
        {
            return idSetting;
        }
        set
        {
            idSetting = value;
        }
    }

    public String ColorCode
    {
        get
        {
            return colorCode;
        }
        set
        {
            colorCode = value;
        }
    }

    public String Encryption
    {
        get
        {
            return encryption;
        }
        set
        {
            encryption = value;
        }
    }

    public String EncryptionType
    {
        get
        {
            return encryptionType;
        }
        set
        {
            encryptionType = value;
        }
    }

    public String EncryptionKey
    {
        get
        {
            return encryptionKey;
        }
        set
        {
            encryptionKey = value;
        }
    }

    public String Promiscuous
    {
        get
        {
            return promiscuous;
        }
        set
        {
            promiscuous = value;
        }
    }

    public String TxAuthorityD
    {
        get
        {
            return txAuthorityD;
        }
        set
        {
            txAuthorityD = value;
        }
    }

    public String KillCode
    {
        get
        {
            return killCode;
        }
        set
        {
            killCode = value;
        }
    }

    public String WakeUpCode
    {
        get
        {
            return wakeUpCode;
        }
        set
        {
            wakeUpCode = value;
        }
    }

    public String Contacts
    {
        get
        {
            return contacts;
        }
        set
        {
            contacts = value;
        }
    }

    public String RxGroupsList
    {
        get
        {
            return rxGroupsList;
        }
        set
        {
            rxGroupsList = value;
        }
    }

    public String Gps
    {
        get
        {
            return gps;
        }
        set
        {
            gps = value;
        }
    }

    public String SendGpsInfo
    {
        get
        {
            return sendGpsInfo;
        }
        set
        {
            sendGpsInfo = value;
        }
    }

    public String ReceiveGpsInfo
    {
        get
        {
            return receiveGpsInfo;
        }
        set
        {
            receiveGpsInfo = value;
        }
    }

    public String GpsTimingReport
    {
        get
        {
            return gpsTimingReport;
        }
        set
        {
            gpsTimingReport = value;
        }
    }

    public String GpsTimingReportTxContacts
    {
        get
        {
            return gpsTimingReportTxContacts;
        }
        set
        {
            gpsTimingReportTxContacts = value;
        }
    }

    public Channel DeepCopy()
    {
        Channel othercopy = (Channel)this.MemberwiseClone();
        return othercopy;
    }
}
