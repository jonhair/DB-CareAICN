using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpdateDBcareAICN
{
    class Account
    {
        public string acctNum { get; set; }
        public string HIC { get; set; }
        public string ICN { get; set; }
        public string last { get; set; }
        public string first { get; set; }
        public string stat { get; set; }
        public string billAmt { get; set; }
        public string payAmt { get; set; }
        public string checkAmt { get; set; }
        public string tmpDate { get; set; }
        public string EFT { get; set; }

        public Account(string acctNum, string HIC, string ICN, string last, string first, string stat, string billAmt, string payAmt, string checkAmt, string tmpDate, string EFT)
        {
            this.acctNum = acctNum;
            this.HIC = HIC;
            this.ICN = ICN;
            this.last = last;
            this.first = first;
            this.stat = stat;
            this.billAmt = billAmt;
            this.payAmt = payAmt;
            this.checkAmt = checkAmt;
            this.tmpDate = tmpDate;
            this.EFT = EFT;
        }
    }
}
