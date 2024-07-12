using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NopStation.Plugin.Misc.AmazonS3.Models
{
    public enum CannedACLEnum
    {
        NoACL = 1,

        Private = 2,

        PublicRead = 3,

        PublicReadWrite = 4,

        AuthenticatedRead = 5,

        AWSExecRead = 6,

        BucketOwnerRead = 7,

        BucketOwnerFullControl = 8,

        LogDeliveryWrite = 9
    }
}
