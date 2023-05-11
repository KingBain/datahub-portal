﻿using System;

namespace Datahub.Core.Model.Announcements;

public class Announcement
{
    public int Id { get; set; }
    public string TitleEn { get; set; }
    public string TitleFr { get; set; }
    public string PreviewEn { get; set; }
    public string PreviewFr { get; set; }
    public string BodyEn { get; set; }
    public string BodyFr { get; set; }

    public bool ForceHidden { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime? EndDateTime { get; set; }

    public bool IsNew() => Id == 0;
    public bool IsVisible() => !IsDeleted && !ForceHidden;
}
