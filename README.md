# BookmarksManager

## Summary

Add and remove browser bookmarks from a central repository.

![image](https://user-images.githubusercontent.com/17654421/63959925-9d505d80-ca8d-11e9-9251-0dac84465997.png)

May be used in corporate environment : Drop software into a network share, anybody will be able to add and remove bookmark.

## Supported browser

- Microsoft Internet Explorer
- Google Chrome
- Mozilla Firefox (ESR and Release)

## Configuration

### Set bookmark folder name (Chrome and Firefox) 

Edit BookmarkFolderName into BookmarksManager.exe.config.

### Set used Firefox version

Edit FirefoxVersion into BookmarksManager.exe.config.

Version may be :

- esr
- release

### Add and remove bookmarks from repository. 

Add bookmarks to BookmarkManager by editing bookmarkList.xml :

```xml
<bookmarks>
  <bookmark firefox="true" chrome="true" ie="true" url="https://yandex.com" name="Yandex"/>
  <bookmark firefox="true" chrome="true" ie="false" url="https://mail.yandex.ru" name="Yandex.Mail"/>
</bookmarks>
```

- mandatory string name
- mandatory string url 
- optional bool firefox default=true
- optional bool chrome default=true
- optional bool ie default=true

Setting one of firefox, chrome or IE to false will disable bookmark for the selected browser into BookmarkManager.
