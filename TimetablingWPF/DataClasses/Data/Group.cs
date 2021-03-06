﻿using System;
using System.Collections.Generic;
using System.IO;
using TimetablingWPF;
using System.Linq;

namespace TimetablingWPF
{
    public class Group : BaseDataClass
    {
        static Group()
        {
            Type type = typeof(Group);
            RegisterProperty(type, nameof(Subjects));
            RegisterProperty(type, nameof(Rooms));
        }
        public RelationalCollection<Subject, Group> Subjects { get; private set; } = new RelationalCollection<Subject, Group>(nameof(Subject.Groups));
        private int rooms;
        public int Rooms
        {
            get => rooms;
            set
            {
                if (rooms != value)
                {
                    rooms = value;
                    NotifyPropertyChanged(nameof(Rooms));
                }
            }
        }
        public override void SaveChild(BinaryWriter writer)
        {
            Saving.WriteIntEnum(Subjects.Select(s => s.StorageIndex), writer);
            writer.Write(Rooms);
        }

        public override void LoadChild(BinaryReader reader, Version version, DataContainer container)
        {
            Loading.LoadEnum(() => Subjects.Add(container.Subjects[reader.ReadInt32()]), reader);
            rooms = reader.ReadInt32();
        }
    }
}
