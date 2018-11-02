'use strict';

if(!window.key) {
  alert('请填写密钥');
  throw '请填写密钥';
}

const url = 'https://eastaisa.api.cognitive.microsoft.com/face/v1.0/detect';
const params = {
  // Request parameters
  'returnFaceId': false,
  'returnFaceLandmarks': false,
  'returnFaceAttributes': 'age,gender,smile,emotion'
};

const video = document.querySelector('video');
const canvas = document.querySelector('canvas');
canvas.width = 480;
canvas.height = 360;

const button = document.querySelector('button');
button.onclick = function () {
  canvas.width = video.videoWidth;
  canvas.height = video.videoHeight;
  canvas.getContext('2d')
    .drawImage(video, 0, 0, canvas.width, canvas.height);
  videoTracks.forEach(track => track.stop());
  let data = canvas.toDataURL('image/png');
  fetch(data)
    .then(res => res.blob())
    .then(blobData => {
      $.post({
        url: "https://eastasia.api.cognitive.microsoft.com/face/v1.0/detect?" + $.param(params),
        contentType: "application/octet-stream",
        headers: {
          'Ocp-Apim-Subscription-Key': key
        },
        processData: false,
        data: blobData
      })
      .done(res => console.log(res))
      .fail(err => console.log(err));
    });
}

let videoTracks;
const constraints = {
  audio: false,
  video: true
};

function handleSuccess(stream) {
  video.srcObject = stream;
  videoTracks = stream.getVideoTracks();
}

function handleError(error) {
  console.log('navigator.getuserMedia error: ', error);
}

navigator.mediaDevices.getUserMedia(constraints)
  .then(handleSuccess)
  .catch(handleError);