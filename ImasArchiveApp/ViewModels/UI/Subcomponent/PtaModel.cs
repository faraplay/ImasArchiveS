using System.Collections.ObjectModel;

namespace ImasArchiveApp
{
    public class PtaModel : SubcompPart<PtaElementModel>
    {
        protected override int VisibilityIndex => 1;
        public ObservableCollection<UISpriteSheetModel> SpriteSheets { get; }

        public PtaModel(UISubcomponentModel subcomponentModel) : base(subcomponentModel)
        {
            SpriteSheets = new ObservableCollection<UISpriteSheetModel>();
            for (int i = 0; i < subcomponentModel.Subcomponent.imageSource.Count; i++)
            {
                SpriteSheets.Add(new UISpriteSheetModel(subcomponentModel, subcomponentModel.Subcomponent.imageSource.Filenames[i], i, subcomponentModel.GetFileName));
            }
        }

        public void UpdateRectangles()
        {
            foreach (var spritesheet in SpriteSheets)
            {
                spritesheet.UpdateRectangles();
            }
        }
    }
}
