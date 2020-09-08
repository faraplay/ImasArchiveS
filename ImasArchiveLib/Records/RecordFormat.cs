namespace Imas.Records
{
    internal class RecordFormat
    {
        public static readonly RecordFormat[] formats = {
            new RecordFormat("parameter/accessory.bin", "accessory", "iiiiibbbbiic020c080bbbbbbbbii"),
            new RecordFormat("parameter/album.bin", "album", "ssbbsbbsssa010c020c020c020"),
            new RecordFormat("parameter/albumCommu.bin", "albumCommu", "ssbbsbbsssa010c020c020c020"),
            new RecordFormat("parameter/costume_par/ps3.out", "costume_ps3", "iiiia010c020c080bbsbbsssiibbbb"),
            new RecordFormat("parameter/dlcName/dlcName.bin", "dlcName", "bbsa010c040"),
            new RecordFormat("parameter/item.bin", "item", "isbbiic020c080c080ssssssiiiibbbb"),
            new RecordFormat("parameter/lesson_par/menu.bin", "lesson_menu", "bbssssc020c040ssssssssssi"),
            //new RecordFormat("parameter/mail_idol_par/_ps3_mail_idol.bin", "mail_idol", "ssic020c800"),
            //new RecordFormat("parameter/mail_idol_par/_dlc01_mail_idol.bin", "mail_idol", "ssic020c800"),
            //new RecordFormat("parameter/mail_system_par/mail_system.bin", "mail_system", "ssic020c800"),
            new RecordFormat("parameter/money.bin", "money", "ssbbsic020"),
            new RecordFormat("parameter/nonUnit/nonUnitFanUp.bin", "nonUnitFanUp", "bbsic080"),
            new RecordFormat("parameter/producerRank/producerRank.bin", "producerRank", "bbbbbbsbbsc040"),
            new RecordFormat("parameter/profile.bin", "profile", "sbbbbbbbbbbbbbba010c020c020c020c020c020c020c020c040c080c080"),
            new RecordFormat("parameter/reporter.bin", "reporter", "iisssbbc020c040"),
            new RecordFormat("parameter/season/seasonText.bin", "seasonText", "bbsc040c020c020"),
            new RecordFormat("work/workInfo.bin", "workInfo", "sbbbbbbbbbbiiiisbbssssssssbbsbbssbbssbbbbisbbsssbbbbsc020c020c100"),
            new RecordFormat("work/rivalInfo.bin", "rivalInfo", "sbbbbbbssssbbbbbbbbiiiiiiiibbbbssc040c080c020")
        };

        public string fileName;
        public string sheetName;
        public string format;

        public RecordFormat(string fileName, string sheetName, string format)
        {
            this.fileName = fileName;
            this.sheetName = sheetName;
            this.format = format;
        }
    }
}