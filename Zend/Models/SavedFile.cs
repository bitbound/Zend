using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Zend.Models
{
    public class SavedFile
    {
        public string ContentDisposition { get; init; }

        public string ContentType { get; init; }

        public string FileName { get; init; }

        public long FileSize { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; init; }
        public DateTimeOffset UploadedAt { get; init; }
    }
}
