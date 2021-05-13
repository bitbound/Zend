import React, { Component } from 'react';
import Card from 'reactstrap/lib/Card';
import CardBody from 'reactstrap/lib/CardBody';
import { Upload } from '../models/Upload';
import "./Home.css";

/* Wow.  React without TypeScript is really not fun.  */

export class Home extends Component {
  static displayName = Home.name;

  hiddenFileInput = React.createRef();

  constructor() {
    super();
    this.state = {
      uploads: []
    }
  }

  componentDidMount() {
    window.addEventListener("dragover", ev => {
      ev.preventDefault();
      ev.dataTransfer.dropEffect = "copy";
    });

    window.addEventListener("drop", ev => {
      if (ev.dataTransfer.files.length < 1) {
        return;
      }
      this.uploadFiles(ev.dataTransfer.files);
    });

    this.hiddenFileInput.current.addEventListener("change", ev => {
      this.uploadFiles(ev.target.files);
    });
  }

  componentWillUnmount() {
    window.removeEventListener("dragover");
    window.removeEventListener("drop");
  }

  openFilePrompt = () => {
    if (this.hiddenFileInput?.current) {
      this.hiddenFileInput.current.click();
    }
  }

  uploadFiles = (fileList) => {
    if ((fileList || []).length == 0) {
      return;
    }

    console.log(fileList);

    for (var i = 0; i < fileList?.length; i++) {
      var file = fileList[i];
      var upload = new Upload(file.name, file.size, new Date());

      var fd = new FormData();
      fd.append('file', file);
      var xhr = new XMLHttpRequest();
      xhr.open('POST', `${window.location.origin}/api/file`, true);

      xhr.addEventListener("load", function () {
          if (xhr.status === 200) {
              upload.url = xhr.responseText;
          }
          else {
              alert(`File upload failed. Status code: ${xhr.status}`);
          }
      });
      
      xhr.upload.addEventListener("progress", function (e) {
        upload.percentUploaded = e.loaded / file.size;
      });
      xhr.send(fd);
    }
  }

  render() {
    return (
      <div className="text-center">
        <div className="d-inline-block">

          <div className="dropzone" onClick={this.openFilePrompt}>
            <Card className="dropcard">
              <CardBody>
                <div className="droptext">
                  Drop files or click here.
              </div>
              </CardBody>
            </Card>
          </div>

          <div className="text-left">
            <h5 className="text-left mt-5">Uploads</h5>

            <div className="searchbox mt-2">
              <label>Search</label>
              <br />
              <input type="text" className="form-control"></input>
            </div>
          </div>

          <div className="text-left mt-3">
            {this.renderUploads()}
          </div>

        </div>

        <input ref={this.hiddenFileInput} type="file" hidden></input>
      </div>
    );
  }

  renderUploads() {
    if (!this.state.uploads || this.state.uploads.length == 0) {
      return "No uploads yet.";
    }

    this.state.uploads.map(x => {
      var percentUploaded = Math.round(x.percentUploaded * 100).toString() + "%";
      return (
        <div>
          <div>x.fileName</div>
          <div>x.fileSize</div>
          <div>x.uploadDate</div>
          <div>x.url</div>
          <div>percentUploaded</div>
        </div>
      )
    })
  }
}
