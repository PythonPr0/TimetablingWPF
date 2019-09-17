﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Humanizer;
using System.Diagnostics;

namespace TimetablingWPF
{
    /// <summary>
    /// A list which reflects updates in itself with the list in the added class
    /// </summary>
    /// <typeparam name="T">The type of the objects in this list</typeparam>
    public class RelationalList<T> : ObservableCollection<T>, ICloneable
    {
        /// <summary>
        /// The object to which this list belongs
        /// </summary>
        public object Parent { get; set; }
        /// <summary>
        /// Holds the field name of the object of type T which holds the RelationalList which will hold the parent
        /// </summary>
        private readonly string OtherClassField;
        /// <summary>
        /// The class constructor
        /// </summary>
        /// <param name="otherClassProperty"><see cref="OtherClassField"/></param>
        /// <param name="parent"><see cref="Parent"/></param>
        public RelationalList(string otherClassProperty,
            BaseDataClass parent = null)
        {
            OtherClassField = otherClassProperty;
            Parent = parent;
        }
        /// <summary>
        /// Adds a new item to the list, and adds the parent in the list of the added item
        /// </summary>
        /// <param name="item">Item to add to the list</param>
        public new void Add(T item)
        {
            base.Add(item);

            ((IList)item.GetType().GetProperty(OtherClassField).GetValue(item)).Add(Parent);
        }
        /// <summary>
        /// Shallow clone of object
        /// </summary>
        /// <returns> An object that is a shallow copy of this one</returns>
        public object Clone()
        {
            return MemberwiseClone();
        }

        public new void Remove(T item)
        {
            base.Remove(item);
            ((IList)item.GetType().GetProperty(OtherClassField).GetValue(item)).Remove(Parent);
        }
    }
    /// <summary>
    /// Holds data about a timetabling slot
    /// </summary>
    public struct TimetableSlot
    {
        public int Week, Day, Period;
        public TimetableSlot(int week, int day, int period)
        {
            Week = week;
            Day = day;
            Period = period;
        }
    }
    /// <summary>
    /// Represents an assignment between a class and a teacher
    /// </summary>
    public class Assignment
    {
        public Teacher Teacher;
        public Class Class;
        public int Periods;
        /// <summary>
        /// The string representation of this object from a teacher's perspective
        /// </summary>
        public string TeacherString;
        /// <summary>
        /// The string representation of this object from a class' perspective
        /// </summary>
        public string ClassString;
        /// <summary>
        /// A constructor for when the class is not known
        /// </summary>
        /// <param name="teacher"></param>
        /// <param name="periods"></param>
        public Assignment(Teacher teacher, int periods)
        {
            Teacher = teacher;
            Periods = periods;
            ClassString = $"{Teacher}: {Periods}";
        }
        /// <summary>
        /// A constructor for when the teacher is not known
        /// </summary>
        /// <param name="class"></param>
        /// <param name="periods"></param>
        public Assignment(Class @class, int periods)
        {
            Class = @class;
            Periods = periods;
            TeacherString = $"{Class}: {Periods}";
        }
        /// <summary>
        /// Creates references to this assignment in the teacher and class assignments list
        /// </summary>
        /// <param name="teacher"></param>
        /// <exception cref="System.InvalidOperationException">This will be thrown if the teacher is already defined</exception>
        public void Commit(Teacher teacher)
        {
            if (Class == null)
            {
                throw new InvalidOperationException("Commit should be called with a class, as the class has not been set");
            }
            Teacher = teacher;
            Teacher.Assignments.Add(this);
            Class.Assignments.Add(this);
        }
        /// <summary>
        /// Creates references to this assignment in the teacher and class assignments list
        /// </summary>
        /// <param name="@class"></param>
        /// <exception cref="InvalidOperationException">This will be thrown if the class is already defined</exception>
        public void Commit(Class @class)
        {
            if (Teacher == null)
            {
                throw new InvalidOperationException("Commit should be called with a teacher, as the teacher has not been set");
            }
            Class = @class;
            Teacher.Assignments.Add(this);
            Class.Assignments.Add(this);
        }

        public new string ToString()
        {
            return $"{Teacher}: {Class} ({Periods})";
        }
    }
    /// <summary>
    /// Base class for all data objects
    /// </summary>
    public abstract class BaseDataClass : INotifyPropertyChanged, ICloneable
    {

        public BaseDataClass()
        {
            ApplyOnType<RelationalList<object>>((prop, val) => ((RelationalList<object>)val).Parent = this);
        }
        public string Name
        {
            get { return _Name; }
            set
            {
                if (value != _Name)
                {
                    _Name = value;
                    NotifyPropertyChanged("Name");
                }
            }
        }
        /// <summary>
        /// Holder for Name
        /// </summary>
        private string _Name;
        protected abstract string ListName { get; }
        private bool Commited = false;
        /// <summary>
        /// Add this to its associated list in properties. Is idempotent.
        /// </summary>
        public virtual void Commit()
        {
            if (Commited)
            {
                return;
            }
            ((IList)Application.Current.Properties[ListName]).Add(this);
            Commited = true;
        }
        /// <summary>
        /// Event when property is changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public override string ToString()
        {
            return Name;
        }
        /// <summary>
        /// A helper method in the class to automatically set the parent on a nullable RelationalList
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rlist"></param>
        /// <returns cref="RelationalList{T}"></returns>
        protected RelationalList<T> NewRL<T>(RelationalList<T> rlist)
        {
            if (rlist == null) { return null; }
            rlist.Parent = this;
            return rlist;
        }
        /// <summary>
        /// Will remove all instances of self from <see cref="RelationalList{T}"/>. Will then remove self from the properties list
        /// </summary>
        public void Delete()
        {
            throw new NotImplementedException();
        }

        public object Clone()
        {
            object copy = MemberwiseClone();
            ApplyOnType<IEnumerable>((prop, val) => prop.SetValue(copy, ((ICloneable)val).Clone()));
            return copy;
        }

        private void ApplyOnType<T>(Action<System.Reflection.PropertyInfo, object> action)
        {
            foreach (System.Reflection.PropertyInfo prop in GetType().GetProperties())
            {
                object val = prop.GetValue(this);
                if (val is T)
                {
                    action(prop, val);
                }
            };
        }
    }

    public class Room : BaseDataClass
    {
        public Room(string name, int quantity, RelationalList<Subject> subjects = null)
        {
            Name = name;
            Quantity = quantity;
            Subjects = NewRL(subjects) ?? new RelationalList<Subject>("Rooms", this);
        }
        private int _Quantity;
        public int Quantity
        {
            get { return _Quantity; }
            set
            {
                if (value != _Quantity)
                {
                    _Quantity = value;
                    NotifyPropertyChanged("Quantity");
                }
            }
        }
        public RelationalList<Subject> Subjects { get; }

        protected override string ListName => "Rooms";
    }

    public class Teacher : BaseDataClass
    {
        public ObservableCollection<TimetableSlot> UnavailablePeriods { get; } = new ObservableCollection<TimetableSlot>();
        public RelationalList<Subject> Subjects { get; } = new RelationalList<Subject>("Teachers");
        public ObservableCollection<Assignment> Assignments { get; } = new ObservableCollection<Assignment>();
        protected override string ListName => "Teachers";
    }

    public class Subject : BaseDataClass
    {
        public Subject(string name, RelationalList<Room> rooms = null, RelationalList<Teacher> teachers = null)
        {
            Name = name;
            Rooms = NewRL(rooms) ?? new RelationalList<Room>("Subjects", this);
            Teachers = NewRL(teachers) ?? new RelationalList<Teacher>("Subjects", this);
        }
        public RelationalList<Room> Rooms { get; }
        public RelationalList<Teacher> Teachers { get; }
        protected override string ListName => "Subjects";
    }

    public class Group : BaseDataClass
    {
        public Group(string name, RelationalList<Class> classes = null)
        {
            Name = name;
            Classes = NewRL(classes) ?? new RelationalList<Class>("Groups", this);
        }
        public RelationalList<Class> Classes { get; }
        protected override string ListName => "Groups";
    }

    public class Class : BaseDataClass
    {
        public Class(string name, Subject subject, int lessonsPerCycle,
            ObservableCollection<Assignment> assignments = null, RelationalList<Group> groups = null, int lessonLength = 1)
        {
            Name = name;
            Subject = subject;
            LessonsPerCycle = lessonsPerCycle;
            Assignments = assignments ?? 
                new ObservableCollection<Assignment>();
            Groups = NewRL(groups) ?? new RelationalList<Group>("Classes", this);
            LessonLength = lessonLength;
        }
        private Subject _Subject;
        public Subject Subject
        {
            get { return _Subject; }
            set
            {
                if (value != _Subject)
                {
                    _Subject = value;
                    NotifyPropertyChanged("Subject");
                }
            }
        }
        private int _LessonsPerCycle;
        public int LessonsPerCycle
        {
            get { return _LessonsPerCycle; }
            set
            {
                if (value != _LessonsPerCycle)
                {
                    _LessonsPerCycle = value;
                    NotifyPropertyChanged("LessonsPerCycle");
                }
            }
        }
        public ObservableCollection<Assignment> Assignments { get; }
        public RelationalList<Group> Groups { get; }
        protected override string ListName => "Classes";
        private int _LessonLength;
        public int LessonLength
        {
            get { return _LessonLength; }
            set
            {
                if (value != _LessonLength)
                {
                    _LessonLength = value;
                    NotifyPropertyChanged("LessonLength");
                }
            }
        }

    }
    public class TimetableStructure
    {
        public TimetableStructure(int weeksPerCycle, IList<TimetableStructurePeriod> structure)
        {
            WeeksPerCycle = weeksPerCycle;
            Structure = structure;
            TotalFreePeriods = weeksPerCycle*5*(from period in structure where period.IsSchedulable select period).Count();
        }
        public int WeeksPerCycle { get; }
        public IList<TimetableStructurePeriod> Structure { get; }
        public int TotalFreePeriods { get; }
    }
    public class TimetableStructurePeriod :  INotifyPropertyChanged
    {
        public TimetableStructurePeriod(string name, bool isSchedulable)
        {
            Name = name;
            IsSchedulable = isSchedulable;
        }
        private string _Name;
        public string Name
        {
            get { return _Name; }
            set
            {
                if (value != _Name)
                {
                    _Name = value;
                    NotifyPropertyChanged("Name");
                }
            }
        }
        private bool _IsSchedulable;
        public bool IsSchedulable
        {
            get { return _IsSchedulable; }
            set
            {
                if (value != _IsSchedulable)
                {
                    _IsSchedulable = value;
                    NotifyPropertyChanged("IsSchedulable");
                }
            }
        }
        public void NotifyPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
