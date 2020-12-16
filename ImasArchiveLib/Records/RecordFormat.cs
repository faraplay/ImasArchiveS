namespace Imas.Records
{
    internal class RecordFormat
    {
        public static readonly RecordFormat[] formats = {
            new RecordFormat("parameter/accessory.bin", "accessory", "iiiiibbbbiic020c080bbbbbbbbii", new string[]
                {
                    "ID", "", "Order", "", "", "Type", "Dance", "Visual", "Vocal", "Price", "",
                    "Name", "Description",
                    "", "", "", "", "",
                    "", "", "", "", "",
                }),
            new RecordFormat("parameter/album.bin", "album", "ssbbsbbsssa010c020c020c020", new string[]
                { 
                    "ID", "Idol", "Movie", "Order", "", "", "", "Order again", "Song", "", "String ID", "Name", "Idol Name", "Location",
                }),
            new RecordFormat("parameter/albumCommu.bin", "albumCommu", "ssbbsbbsssa010c020c020c020", new string[]
                {
                    "ID", "Idol", "Movie", "Order", "", "", "", "", "", "", "String ID", "Name", "Idol Name", "Location",
                }),
            new RecordFormat("parameter/costume_par/ps3.out", "costume", "iiiia010c020c080bbsbbsssiibbbb", new string[] 
                { 
                    "", "", "", "", "String ID", "Name", "Description", "", "", "", "", "", 
                    "Dance", "Visual", "Vocal", "Price", "", "", "", "", "",
                }),
            new RecordFormat("parameter/costume_par/dlc01.out", "costume", "iiiia010c020c080bbsbbsssiibbbb", new string[]{ "", "", "", "", "String ID", "Name", "Description", "", "", "", "", "", "Dance", "Visual", "Vocal", "Price", "", "", "", "", "", }),
            new RecordFormat("parameter/costume_par/dlc02.out", "costume", "iiiia010c020c080bbsbbsssiibbbb", new string[]{ "", "", "", "", "String ID", "Name", "Description", "", "", "", "", "", "Dance", "Visual", "Vocal", "Price", "", "", "", "", "", }),
            new RecordFormat("parameter/costume_par/dlc03.out", "costume", "iiiia010c020c080bbsbbsssiibbbb", new string[]{ "", "", "", "", "String ID", "Name", "Description", "", "", "", "", "", "Dance", "Visual", "Vocal", "Price", "", "", "", "", "", }),
            new RecordFormat("parameter/costume_par/dlc04.out", "costume", "iiiia010c020c080bbsbbsssiibbbb", new string[]{ "", "", "", "", "String ID", "Name", "Description", "", "", "", "", "", "Dance", "Visual", "Vocal", "Price", "", "", "", "", "", }),
            new RecordFormat("parameter/costume_par/dlc05.out", "costume", "iiiia010c020c080bbsbbsssiibbbb", new string[]{ "", "", "", "", "String ID", "Name", "Description", "", "", "", "", "", "Dance", "Visual", "Vocal", "Price", "", "", "", "", "", }),
            new RecordFormat("parameter/costume_par/dlc06.out", "costume", "iiiia010c020c080bbsbbsssiibbbb", new string[]{ "", "", "", "", "String ID", "Name", "Description", "", "", "", "", "", "Dance", "Visual", "Vocal", "Price", "", "", "", "", "", }),
            new RecordFormat("parameter/costume_par/dlc07.out", "costume", "iiiia010c020c080bbsbbsssiibbbb", new string[]{ "", "", "", "", "String ID", "Name", "Description", "", "", "", "", "", "Dance", "Visual", "Vocal", "Price", "", "", "", "", "", }),
            new RecordFormat("parameter/costume_par/dlc08.out", "costume", "iiiia010c020c080bbsbbsssiibbbb", new string[]{ "", "", "", "", "String ID", "Name", "Description", "", "", "", "", "", "Dance", "Visual", "Vocal", "Price", "", "", "", "", "", }),
            new RecordFormat("parameter/costume_par/dlc09.out", "costume", "iiiia010c020c080bbsbbsssiibbbb", new string[]{ "", "", "", "", "String ID", "Name", "Description", "", "", "", "", "", "Dance", "Visual", "Vocal", "Price", "", "", "", "", "", }),
            new RecordFormat("parameter/costume_par/dlc10.out", "costume", "iiiia010c020c080bbsbbsssiibbbb", new string[]{ "", "", "", "", "String ID", "Name", "Description", "", "", "", "", "", "Dance", "Visual", "Vocal", "Price", "", "", "", "", "", }),
            new RecordFormat("parameter/costume_par/dlc11.out", "costume", "iiiia010c020c080bbsbbsssiibbbb", new string[]{ "", "", "", "", "String ID", "Name", "Description", "", "", "", "", "", "Dance", "Visual", "Vocal", "Price", "", "", "", "", "", }),
            new RecordFormat("parameter/costume_par/dlc12.out", "costume", "iiiia010c020c080bbsbbsssiibbbb", new string[]{ "", "", "", "", "String ID", "Name", "Description", "", "", "", "", "", "Dance", "Visual", "Vocal", "Price", "", "", "", "", "", }),
            new RecordFormat("parameter/costume_par/dlc13.out", "costume", "iiiia010c020c080bbsbbsssiibbbb", new string[]{ "", "", "", "", "String ID", "Name", "Description", "", "", "", "", "", "Dance", "Visual", "Vocal", "Price", "", "", "", "", "", }),
            new RecordFormat("parameter/costume_par/dlc14.out", "costume", "iiiia010c020c080bbsbbsssiibbbb", new string[]{ "", "", "", "", "String ID", "Name", "Description", "", "", "", "", "", "Dance", "Visual", "Vocal", "Price", "", "", "", "", "", }),
            new RecordFormat("parameter/dlcName/dlcName.bin", "dlcName", "bbsa010c040", new string[]
                {
                    "", "", "", "String ID", "Name",
                }),
            new RecordFormat("parameter/item.bin", "item", "isbbiic020c080c080ssssssiiiibbbb", new string[]
                {
                    "", "", "", "", "Price", "", "Name", "Description", "Use Text", "", "", "", "",
                    "", "", "Money", "", "", "", "", "", "", "",
                }),
            new RecordFormat("parameter/lesson_par/menu.bin", "lesson_menu", "bbssssc020c040ssssssssssi", new string[]
                {
                    "ID", "Type", "Cost", "", "Difficulty", "", "Name", "Description",
                    "Bad Effect", "Normal Effect", "Good Effect", "Perfect Effect", "Bonus Effect",
                    "Bad Duration", "Normal Duration", "Good Duration", "Perfect Duration", "Bonus Duration",
                    ""
                }),
            new RecordFormat("parameter/mail_system_par/mail_system.bin", "mail_system", "ssic020c800", new string[]
                {
                    "ID", "Image", "", "Subject", "Message",
                }),
            new RecordFormat("parameter/money.bin", "money", "ssbbsic020", null),
            new RecordFormat("parameter/nonUnit/nonUnitFanUp.bin", "nonUnitFanUp", "bbsic080", new string[]
                {
                    "", "Has Text", "", "", "Text",
                }),
            new RecordFormat("parameter/producerRank/producerRank.bin", "producerRank", "bbbbbbsbbsc040", new string[]
                {
                    "Level", "", "", "", "", "", "", "", "", "", "Title",
                }),
            new RecordFormat("parameter/profile.bin", "profile", "sbbbbbbbbbbbbbba010c020c020c020c020c020c020c020c040c080c080", new string[]
                {
                    "ID", "Gender", "Age", "Height", "Weight", "Breast", "Waist", "Hip", "Month", "Day", "Image Type", "Zodiac", "", "", "",
                    "String ID", "Given Name", "Surname", "Given Name Reading", "Surname Reading", "Hobby1", "Hobby2", "Hobby3", 
                    "Title", "Self Description", "Kotori Memo",
                }),
            new RecordFormat("parameter/reporter.bin", "reporter", "iisssbbc020c040", new string[]
                {
                    "ID", "", "Fan Effect", "Money Effect", "EXP Effect", "", "Number of weeks", "Name", "Description",
                }),
            new RecordFormat("parameter/season/seasonText.bin", "seasonText", "bbsc040c020c020", new string[]
                {
                    "ID", "", "", "Text 1", "Text 2", "Counter Word",
                }),
            new RecordFormat("work/workInfo.bin", "workInfo", "sbbbbbbbbbbiiiisbbssssssssbbsbbssbbssbbbbisbbsssbbbbsc020c020c100", new string[]
                {
                    "", "", "", "", "", "", "", "", "", "",
                    "", "", "", "", "", "", "", "", "", "",
                    "", "", "", "", "", "", "", "", "", "",
                    "", "", "", "", "", "", "", "", "", "",
                    "", "", "", "", "", "", "", "", "", "",
                    "", "", "",
                    "Name", "Location", "Description",
                }),
            new RecordFormat("work/rivalInfo.bin", "rivalInfo", "sbbbbbbssssbbbbbbbbiiiiiiiibbbbssc040c080c020", new string[]
                {
                    "", "", "", "", "", "", "", "", "", "",
                    "", "", "", "", "", "", "", "", "", "",
                    "", "", "", "", "", "", "", "", "", "",
                    "", "", "",
                    "Skill", "Description", "Name",
                })
        };

        public string fileName;
        public string sheetName;
        public string format;
        public string[] headers;

        public RecordFormat(string fileName, string sheetName, string format, string[] headers)
        {
            this.fileName = fileName;
            this.sheetName = sheetName;
            this.format = format;
            this.headers = headers;
        }
    }
}