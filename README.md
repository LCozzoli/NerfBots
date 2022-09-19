# Nerf Rust  bots game events
*I am the developer of rustLink, surprised that nobody has done this plugin before, I want to give server owners the opportunity to nerf bots if they desire to.*

If you want to configure the nerf using the chat command you need to grand the permission **nerfbots.configuration**
```
oxide.grant group admin nerfbots.configuration
oxide.grant user <name> nerfbots.configuration
```
Then use this **chat command**, it can also be setup in the **rcon** console.
```
/nerfbots <all|crates|cargo|heli|ch47|explosions> <display|remove>
```

You can also configure the plugin using the config file
```
{
  "Enable debug logs": false,
  "Remove every map markers from rust  (except vending machines)": false,
  "Remove map markers from hackable crates (Nerf oilrigs, cargo crates and CH47 dropped crate)": false,
  "Remove map marker from explosions (Heli crash, Bradley)": false,
  "Remove map marker from cargo ship": true,
  "Remove map marker from helicopter": false,
  "Remove map marker from CH47 (Nerf oilrigs and CH47 dropped crate)": false
}
```
Setting true to an option will **enable the nerf** on it and hide the map marker from the rust  companion app and bots.