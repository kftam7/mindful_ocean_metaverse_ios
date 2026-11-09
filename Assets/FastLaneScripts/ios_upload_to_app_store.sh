# xker2001@gmail.com - is the user to use to upload, must be a user in the App Store Connect list
# qdiq-exkp-sdvg-ewag - is the password generated in https://appleid.apple.com/account/manage

# Upload the build to iTunes Connect
xcrun altool --upload-app -f $WORKSPACE/.build/last/dev-ios/build.ipa -u xker2001@gmail.com -p qdiq-exkp-sdvg-ewag