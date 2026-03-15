# Restaurant Search Chatbot - Quick Reference Card

## ?? START HERE

### 1. Start Applications
```bash
API:  https://localhost:7179
MVC:  https://localhost:7227
```

### 2. Open Dashboard
```
https://localhost:7227/Home/Dashboard
```

### 3. Try a Search
```
"Italian restaurants in Amsterdam"
```

---

## ?? What to Say

### By City
```
? "Restaurants in Paris"
? "Pizza in London"
? "Cheap food in Berlin"
? "Seafood in Barcelona"
? "Italian cuisine in Rome"
```

### By Location
```
? "Find restaurants near me"
? "Pizza nearby"
? "Food close to me"
```

### Just Ask
```
? "What restaurants are there?"
? "Show me some food options"
? "Any pizzerias around?"
```

---

## ?? How It Works

```
Your Input
    ?
Extract location (if any)
    ?
Geocode to coordinates
    ?
Search OpenStreetMap
    ?
Return 5 results with maps
```

---

## ?? Supported Features

| Feature | Status |
|---------|--------|
| City search | ? Works |
| Geolocation | ? Works |
| Google Maps links | ? Works |
| Multi-language | ? English, Dutch |
| Error handling | ? Helpful messages |
| Mobile friendly | ? Responsive |

---

## ?? Geolocation Blocked?

**No problem!** Just use city names:
```
Instead of: "Find restaurants near me"
Try:        "Restaurants in Amsterdam"
```

**To reset permission:**
1. Click ?? lock in address bar
2. Find "Location" ? Reset/Clear
3. Refresh page
4. Click "Allow"

---

## ?? Troubleshooting

| Issue | Solution |
|-------|----------|
| No results | Try a bigger city |
| Slow response | Overpass API is busy, retry |
| Geolocation blocked | Use city names instead |
| API error | Check if API is running |

---

## ?? Documentation Files

- `QUICK_START_WORKING.md` - Full startup guide
- `API_REFERENCE_COMPLETE.md` - Technical API docs
- `GEOLOCATION_TROUBLESHOOTING.md` - Location permission fixes
- `RESTAURANT_SEARCH_FIXES.md` - Implementation details

---

## ?? External APIs (All Free!)

- **Nominatim**: City ? Coordinates
- **Overpass**: Restaurant database
- **Google Maps**: Map links

No API keys required!

---

## ?? What You Get

```
1. Restaurant Name
   https://maps.google.com/?q=Name

2. Restaurant Name
   https://maps.google.com/?q=Name

3. Restaurant Name
   https://maps.google.com/?q=Name

(up to 5 results)
```

---

## ?? Common Searches

| What You Want | What to Say |
|---|---|
| Pizza | "Pizza in [city]" |
| Cheap food | "Cheap restaurants in [city]" |
| Italian | "Italian restaurants in [city]" |
| By location | "Restaurants near me" |
| Default | "Show restaurants" |

---

## ?? Supported Languages

- ???? English
- ???? Dutch

(Switch in top-right corner)

---

## ? Status

- Build: ? Successful
- API: ? Running
- MVC: ? Running
- Search: ? Working
- Geolocation: ? Working (with permission)

---

## ?? Quick Help

**Q: Can I search without location permission?**
A: Yes! Use city names: "Restaurants in Paris"

**Q: Do I need an API key?**
A: No! All services are free and public.

**Q: How many results?**
A: Up to 5 per search.

**Q: What cities work?**
A: All major cities worldwide!

**Q: Is it mobile friendly?**
A: Yes! Fully responsive design.

---

## ?? Next Steps

1. ? Start API (`https://localhost:7179`)
2. ? Start MVC (`https://localhost:7227`)
3. ? Open Dashboard (`/Home/Dashboard`)
4. ? Search: "Italian restaurants in Amsterdam"
5. ? Click a Google Maps link
6. ? Enjoy! ???

---

**Version**: 1.0
**Status**: Production Ready ?
**Last Updated**: December 2024
