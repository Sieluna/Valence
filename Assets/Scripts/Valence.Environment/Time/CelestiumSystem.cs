using Unity.Mathematics;
using UnityEngine;

namespace Valence.Environment
{
    [CreateAssetMenu(fileName = nameof(CelestiumSystem), menuName = "Time/Create Celestium System")]
    public class CelestiumSystem : System
    {
        [SerializeField] private CelestiumComponent m_CelestiumData;
        [SerializeField] private TimeComponent m_TimeData;

        private quaternion m_SunRotation;
        private quaternion m_MoonRotation;

        public override void OnSystemUpdate()
        {
            m_CelestiumData.SunLocalDirection = math.rotate(m_SunRotation, math.forward());
            m_CelestiumData.MoonLocalDirection = math.rotate(m_MoonRotation, math.forward());
        }

        public override void OnSystemFixedUpdate()
        {
            switch (m_CelestiumData.SimulationType)
            {
                case CelestiumSimulationType.Simple:
                default:
                    (m_SunRotation, m_MoonRotation) = SimulateSimpleCelestialCoordinates(m_TimeData);
                    break;
                case CelestiumSimulationType.Realistic:
                    (m_SunRotation, m_MoonRotation) = SimulateRealisticCelestialCoordinates(m_TimeData);
                    break;
            }
        }

        /// <summary>
        /// Simple simulate with celestial alignment (The sun, earth, and moon align in a straight line)
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        private (quaternion, quaternion) SimulateSimpleCelestialCoordinates(TimeComponent time)
        {
            var earthTilt = quaternion.Euler(0.0f, math.radians(time.Latitude), math.radians(time.Latitude));
            var timeRotation = quaternion.Euler((time.Time + time.Utc) * (math.PI * 2.0f) / 24.0f - (math.PI / 2.0f), math.PI, 0.0f);

            var sunRotation = math.mul(earthTilt, timeRotation);
            var moonRotation = math.mul(sunRotation, quaternion.Euler(0.0f, -180.0f, 0.0f));

            return (sunRotation, moonRotation);
        }

        /// <summary>
        /// Compute planetary positions <see href="https://www.stjarnhimlen.se/comp/ppcomp.html">Link</see>
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        private (quaternion, quaternion) SimulateRealisticCelestialCoordinates(TimeComponent time)
        {
            var year = time.Year;
            var month = time.Month;
            var date = time.Date;
            var hour = time.Time - time.Utc;
            
            // The timescale
            float timeScale = 367 * year - 7 * (year + (month + 9) / 12) / 4 + 275 * month / 9 + date - 730530;
            timeScale += hour / 24.0f;

            // The Obliquity of the ecliptic, i.e. The tilt of earth's axis of rotation
            var ecliptic = math.radians(23.4393f - 3.563e-7f * timeScale);

            return (CalculateSun(timeScale, ecliptic), CalculateMoon(timeScale, ecliptic));
        }

        private (float, float) CalculatePosition(float meanDistance, float meanAnomaly, float eccentricity)
        {
            var eccentricAnomaly = meanAnomaly + eccentricity * math.sin(meanAnomaly) * (1.0f + eccentricity * math.cos(meanAnomaly));

            var xv = meanDistance * math.cos(eccentricAnomaly) - eccentricity;
            var yv = meanDistance * math.sqrt(1.0f - eccentricity * eccentricity) * math.sin(eccentricAnomaly);

            var distance = math.degrees(math.atan2(yv, xv));
            var trueAnomaly = math.sqrt(xv * xv + yv * yv);

            return (distance, trueAnomaly);
        }

        private float CalculateSiderealTime(float trueAnomaly, float perihelionArgument)
        {
            var hour = m_TimeData.Time - m_TimeData.Utc;
            var meanLongitude = trueAnomaly + perihelionArgument;

            var GMST0 = meanLongitude + 180.0f;
            var GMST = GMST0 + (hour * 15.0f);

            return math.radians(GMST + m_TimeData.Longitude);
        }

        private (float, float) CalculateAzimuthalCoordinates(float siderealTime, float rightAscension, float declination)
        {
            var latitude = math.radians(m_TimeData.Latitude);
            var hourAngle = siderealTime - rightAscension;

            var x = math.cos(hourAngle) * math.cos(declination);
            var y = math.sin(hourAngle) * math.cos(declination);
            var z = math.sin(declination);

            var xhor = x * math.sin(latitude) - z * math.cos(latitude);
            var yhor = y;
            var zhor = x * math.cos(latitude) + z * math.sin(latitude);

            var azimuth = math.atan2(yhor, xhor) + 180.0f;
            var altitude = math.asin(zhor);

            return (azimuth, altitude);
        }

        private quaternion CalculateSun(float timeScale, float ecliptic)
        {
            // Orbital elements of the sun
            var w = 282.9404f + 4.70935e-5f * timeScale;
            var e = 0.016709f - 1.151e-9f * timeScale;
            var M = math.radians(356.0470f + 0.9856002585f * timeScale);

            // Sun's distance (r) and true anomaly (v)
            var (v, r) = CalculatePosition(1.0f, M, e);

            // Sun's true longitude
            var sunLongitude = math.radians(v + w);

            // Convert lonsun, r to ecliptic rectangular geocentric coordinates xs,ys:
            var xs = r * math.cos(sunLongitude);
            var ys = r * math.sin(sunLongitude);

            // To convert this to equatorial, rectangular, geocentric coordinates, compute:
            var xe = xs;
            var ye = ys * math.cos(ecliptic);
            var ze = ys * math.sin(ecliptic);

            // Sun's right ascension (RA) and declination (Decl):
            var RA = math.atan2(ye, xe);
            var Decl = math.atan2(ze, math.sqrt(xe * xe + ye * ye));

            // The sidereal time
            var LST = CalculateSiderealTime(v, w);

            // Azimuthal coordinates
            var (azimuth, altitude) = CalculateAzimuthalCoordinates(LST, RA, Decl);

            return quaternion.Euler(altitude, azimuth, 0.0f);
        }

        private quaternion CalculateMoon(float timeScale, float ecliptic)
        {
            // Orbital elements of the Moon
            var N = math.radians(125.1228f - 0.0529538083f * timeScale);
            var i = math.radians(5.1454f);
            var w = 318.0634f + 0.1643573223f * timeScale;
            var a = 60.2666f; // Earth radius
            var e = 0.054900f;
            var M = math.radians(115.3654f + 13.0649929509f * timeScale);

            // Moon's distance (r) and true anomaly (v)
            var (v, r) = CalculatePosition(1.0f, M, e);

            // Moon's true longitude
            var sunLongitude = math.radians(v + w);
            var sinLongitude = Mathf.Sin(sunLongitude);
            var cosLongitude = Mathf.Cos(sunLongitude);

            // The position in space - for the planets
            var xh = r * (math.cos(N) * cosLongitude - Mathf.Sin(N) * sinLongitude * Mathf.Cos(i));
            var yh = r * (math.sin(N) * cosLongitude + Mathf.Cos(N) * sinLongitude * Mathf.Cos(i));
            var zh = r * (sinLongitude * Mathf.Sin(i));

            // Equatorial coordinates
            var xe = xh;
            var ye = yh * Mathf.Cos(ecliptic) - zh * Mathf.Sin(ecliptic);
            var ze = yh * Mathf.Sin(ecliptic) + zh * Mathf.Cos(ecliptic);

            // Moon's right ascension (RA) and declination (Decl)
            var RA = math.atan2(ye, xe);
            var Decl = math.atan2(ze, math.sqrt((xe * xe) + (ye * ye)));

            // The sidereal time
            var LST = CalculateSiderealTime(v, w);

            // Azimuthal coordinates
            var (azimuth, altitude) = CalculateAzimuthalCoordinates(LST, RA, Decl);

            return quaternion.Euler(altitude, azimuth, 0.0f);
        }
    }
}