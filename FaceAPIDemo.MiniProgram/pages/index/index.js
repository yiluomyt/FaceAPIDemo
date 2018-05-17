// pages/index/index.js
// 定义API终结点
const baseUrl = 'http://localhost:5000'

Page({
  // 页面的初始数据
  data: {
    // 保存FaceId和人脸位置信息
    faces: [],
    // 保存识别到的实体信息
    results: []
  },
  // 步骤图像，并上传到UploadAPI
  takePhoto() {
    let that = this
    // 获取Camera上下文
    const ctx = wx.createCameraContext()
    // 捕捉图像
    ctx.takePhoto({
      quality: 'low',
      success: (res) => {
        // 在捕捉成功后将图片直接上传到Upload API
        wx.uploadFile({
          url: baseUrl + '/api/Face/Upload',
          filePath: res.tempImagePath,
          name: 'Image',
          success: function (res) {
            let obj = JSON.parse(res.data)
            // 保存检测到的人脸数据
            that.setData({
              faces: obj
            })
            // 若检测到人脸就就进一步调用识别API
            if(obj.length > 0)
            {
              that.identifyFace(obj.map(face => face.id))
            }
          }
        })
      }
    })
  },
  // 输出错误信息
  error(e) {
    console.log(e.detail)
  },
  // 调用Identify API
  identifyFace(faceIds) {
    let that = this
    wx.request({
      url: baseUrl + '/api/Face/Identify',
      method: 'POST',
      data: {
        faces: faceIds
      },
      dataType: 'json',
      success: function(res) {
        that.setData({
          results: res.data
        })
      }
    })
  }
})