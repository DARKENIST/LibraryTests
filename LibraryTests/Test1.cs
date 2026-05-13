using ClassLibrary;

namespace LibraryTests
{
    [TestClass]
    public sealed class BasicTests
    {
        [TestMethod]
        public void Test_EmptySchedule_ReturnsFullDaySlots()
        {
            var slots = Calculations.AvailablePeriods(
                Array.Empty<TimeSpan>(),
                Array.Empty<int>(),
                new TimeSpan(8, 0, 0),
                new TimeSpan(16, 0, 0),
                30);
            string[] expected =
            {
                "08:00-08:30", "08:30-09:00", "09:00-09:30", "09:30-10:00",
                "10:00-10:30", "10:30-11:00", "11:00-11:30", "11:30-12:00",
                "12:00-12:30", "12:30-13:00", "13:00-13:30", "13:30-14:00",
                "14:00-14:30", "14:30-15:00", "15:00-15:30", "15:30-16:00"
            };
            CollectionAssert.AreEqual(expected, slots);
        }

        [TestMethod]
        public void Test_BusyCoversWholeDay_NoFreeSlots()
        {
            var slots = Calculations.AvailablePeriods(
                new[] { new TimeSpan(9, 0, 0) },
                new[] { 540 },
                new TimeSpan(9, 0, 0),
                new TimeSpan(18, 0, 0),
                30);
            Assert.AreEqual(0, slots.Length);
        }

        [TestMethod]
        public void Test_OnlyMorningFree_Consultation30()
        {
            var slots = Calculations.AvailablePeriods(
                new[] { new TimeSpan(10, 0, 0) },
                new[] { 90 },
                new TimeSpan(8, 0, 0),
                new TimeSpan(13, 0, 0),
                30);
            string[] expected =
            {
                "08:00-08:30", "08:30-09:00", "09:00-09:30", "09:30-10:00",
                "11:30-12:00", "12:00-12:30", "12:30-13:00"
            };
            CollectionAssert.AreEqual(expected, slots);
        }

        [TestMethod]
        public void Test_OnlyEveningFree_Consultation30()
        {
            var slots = Calculations.AvailablePeriods(
                new[] { new TimeSpan(14, 0, 0) },
                new[] { 60 },
                new TimeSpan(9, 0, 0),
                new TimeSpan(17, 0, 0),
                30);
            string[] expected =
            {
                "09:00-09:30", "09:30-10:00", "10:00-10:30", "10:30-11:00",
                "11:00-11:30", "11:30-12:00", "12:00-12:30", "12:30-13:00",
                "13:00-13:30", "13:30-14:00", "15:00-15:30", "15:30-16:00",
                "16:00-16:30", "16:30-17:00"
            };
            CollectionAssert.AreEqual(expected, slots);
        }

        [TestMethod]
        public void Test_GapLessThanConsultation_NoSlotInGap()
        {
            var slots = Calculations.AvailablePeriods(
                new[] { new TimeSpan(10, 0, 0), new TimeSpan(10, 50, 0) },
                new[] { 30, 30 },
                new TimeSpan(9, 0, 0),
                new TimeSpan(12, 0, 0),
                30);
            string[] expected =
            {
                "09:00-09:30", "09:30-10:00", "11:20-11:50"
            };
            CollectionAssert.AreEqual(expected, slots);
        }

        [TestMethod]
        public void Test_GapExactlyEqualToConsultation_OneSlot()
        {
            var slots = Calculations.AvailablePeriods(
                new[] { new TimeSpan(10, 0, 0), new TimeSpan(11, 0, 0) },
                new[] { 30, 30 },
                new TimeSpan(9, 0, 0),
                new TimeSpan(12, 0, 0),
                30);
            string[] expected =
            {
                "09:00-09:30", "09:30-10:00", "10:30-11:00"
            };
            CollectionAssert.AreEqual(expected, slots);
        }

        [TestMethod]
        public void Test_OverlappingBusyIntervals_Merged()
        {
            var slots = Calculations.AvailablePeriods(
                new[] { new TimeSpan(10, 0, 0), new TimeSpan(10, 30, 0) },
                new[] { 60, 90 },
                new TimeSpan(9, 0, 0),
                new TimeSpan(13, 0, 0),
                30);
            string[] expected =
            {
                "09:00-09:30", "09:30-10:00", "12:00-12:30", "12:30-13:00"
            };
            CollectionAssert.AreEqual(expected, slots);
        }

        [TestMethod]
        public void Test_ConsultationLongerThanAnyFreeGap_Empty()
        {
            var slots = Calculations.AvailablePeriods(
                new[] { new TimeSpan(9, 30, 0), new TimeSpan(10, 50, 0) },
                new[] { 20, 20 },
                new TimeSpan(9, 0, 0),
                new TimeSpan(12, 0, 0),
                60);
            var slots2 = Calculations.AvailablePeriods(
                new[] { new TimeSpan(9, 30, 0), new TimeSpan(11, 0, 0) },
                new[] { 30, 30 },
                new TimeSpan(9, 0, 0),
                new TimeSpan(12, 0, 0),
                90);
            Assert.AreEqual(0, slots2.Length);
        }

        [TestMethod]
        public void Test_BusyAtStartOfDay_NoSlotBefore()
        {
            var slots = Calculations.AvailablePeriods(
                new[] { new TimeSpan(9, 0, 0) },
                new[] { 30 },
                new TimeSpan(9, 0, 0),
                new TimeSpan(10, 0, 0),
                30);
            string[] expected = { "09:30-10:00" };
            CollectionAssert.AreEqual(expected, slots);
        }

        [TestMethod]
        public void Test_ConsultationTimeZero_EmptyResult()
        {
            var slots = Calculations.AvailablePeriods(
                new[] { new TimeSpan(10, 0, 0) },
                new[] { 30 },
                new TimeSpan(9, 0, 0),
                new TimeSpan(11, 0, 0),
                0);
            Assert.AreEqual(0, slots.Length);
        }
    }
}