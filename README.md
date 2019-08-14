# BookmarksManager

Add and remove browser bookmarks from a central repository.

![image](https://user-images.githubusercontent.com/17654421/63036080-a998c000-bebc-11e9-9e39-8323fc85f325.png)

Supported browser are :

- Microsoft Internet Explorer
- Google Chrome
- Mozilla Firefox (ESR and Release)(show FirefoxVersion into BookmarksManager.exe.config)

May be used in corporate environment : Drop software into a network share, anybody will be able to add and remove bookmark.

Add bookmarks to BookmarkManager by editing bookmarkList.xml :

```xml
<bookmarks>
  <bookmark firefox="true" chrome="true" ie="true" url="https://yandex.com" name="Yandex"/>
  <bookmark firefox="true" chrome="true" ie="false" url="https://mail.yandex.ru" name="Yandex.Mail"/>
</bookmarks>
```

name and url are mandatory
firefox, chrome and ie are optional. Setting them to false will disable bookmark for the selected browser into BookmarkManager.
