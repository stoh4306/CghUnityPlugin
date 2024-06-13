#define EXPORT_API __declspec(dllexport) // Visual Studio needs annotating exported functions with this

#include<fstream>
#include<iostream>
#include<sstream>
#include <numeric>  

#include<vector>
#include<thread>
#include<mutex>
#include<memory>

#include <Eigen/Dense>
#include <pcl/io/pcd_io.h>
#include <pcl/io/ply_io.h>
#include <pcl/point_types.h>
#include <pcl/features/normal_3d.h>

#include <opencv2/opencv.hpp>
#ifdef _WIN32
#include <windows.h>
#endif // WINDOWS


#include "mesh.h"

// Link following functions C-style (required for plugins)
extern "C"
{
	float CalcAvgDistanse(pcl::PointCloud<pcl::PointXYZ>::Ptr cloud);

	cv::Mat ReadImage(const wchar_t* filename, int flag)
	{
		FILE* fp = _wfopen(filename, L"rb");
		if (!fp)
		{
			return cv::Mat::zeros(1, 1, CV_8U);
		}
		fseek(fp, 0, SEEK_END);
		long sz = ftell(fp);
		char* buf = new char[sz];
		fseek(fp, 0, SEEK_SET);
		long n = fread(buf, 1, sz, fp);
		cv::_InputArray arr(buf, sz);
		cv::Mat img = imdecode(arr, flag);
		delete[] buf;
		fclose(fp);
		return img;
	}

	bool WriteImage(std::wstring filename, cv::Mat mat)
	{
		FILE* fp = _wfopen(filename.c_str(), L"wb");
		if (!fp)
		{
			return false;
		}
		//auto ext = filename.substr(filename.rfind(L"."));
		//std::string exta;
		//exta.assign(ext.begin(), ext.end());
		std::vector<uchar> buffer;
		cv::imencode(".png", mat, buffer);
		fwrite(&buffer[0], 1,buffer.size(), fp);
		fclose(fp);
		//fseek(fp, 0, SEEK_END);
		//long sz = ftell(fp);
		//char* buf = new char[sz];
		//fseek(fp, 0, SEEK_SET);
		//long n = fread(buf, 1, sz, fp);
		//cv::_InputArray arr(buf, sz);
		//cv::Mat img = imdecode(arr, flag);
		//delete[] buf;
		//fclose(fp);
		//return img;
	}

	//write pointCloud ->.plyFile
	EXPORT_API void write_point_cloud_in_ply(pcl::PointCloud<pcl::PointXYZRGB>::Ptr cloud, const wchar_t* fileName, bool saveBinary)
	{
		int mode = std::ios::out;
		std::ostringstream ss;
		if (saveBinary) {
			mode = std::ios::binary;
			ss << "ply" << "\n"
				<< "format binary_little_endian 1.0" << "\n";
		}
		else {
			ss << "ply" << "\n"
				<< "format ascii 1.0" << "\n";
		}
		std::ofstream outFile(fileName, mode);
		//auto cloud = maincloud;
		int n = cloud->size();
		ss << "comment" << "\n"
			<< "comment" << "\n"
			<< "element vertex " << n << "\n"
			<< "property float x" << "\n"
			<< "property float y" << "\n"
			<< "property float z" << "\n"
			<< "property uchar red " << "\n"
			<< "property uchar green" << "\n"
			<< "property uchar blue" << "\n"
			<< "end_header" << "\n";
		outFile.write((char*)ss.str().c_str(), ss.str().size());
		if (saveBinary) {
			float tp[3];
			uint8_t tc[3];
			for (int i = 0; i < n; ++i)
			{
				tp[0] = cloud->at(i).x;
				tp[1] = cloud->at(i).y;
				tp[2] = cloud->at(i).z;
				outFile.write((char*)tp, sizeof(float) * 3);
				tc[0] = cloud->at(i).r;
				tc[1] = cloud->at(i).g;
				tc[2] = cloud->at(i).b;
				outFile.write((char*)tc, sizeof(uint8_t) * 3);
				//outFile << "\n";
			}
		}
		else {
			for (int i = 0; i < n; ++i)
			{
				outFile << cloud->at(i).x << " "
					<< cloud->at(i).y << " "
					<< cloud->at(i).z << " "

					<< (int)(cloud->at(i).r) << " "
					<< (int)(cloud->at(i).g) << " "
					<< (int)(cloud->at(i).b)
					<< "\n";
			}
		}

		outFile.close();
	}

	//write pointCloud ->.plyFile with normal
	EXPORT_API void write_point_cloud_in_ply_with_normal(pcl::PointCloud<pcl::PointXYZRGBNormal>::Ptr cloud, const wchar_t* fileName, bool saveBinary)
	{
		int mode = std::ios::out;
		std::ostringstream ss;

		if (saveBinary) {
			mode = std::ios::binary;
			ss << "ply" << "\n"
				<< "format binary_little_endian 1.0" << "\n";
		}
		else {
			ss << "ply" << "\n"
				<< "format ascii 1.0" << "\n";
		}
		std::ofstream outFile(fileName, mode);
		int n = cloud->size();
		ss << "comment" << "\n"
			<< "comment" << "\n"
			<< "element vertex " << n << "\n"
			<< "property float x" << "\n"
			<< "property float y" << "\n"
			<< "property float z" << "\n"
			<< "property uchar red " << "\n"
			<< "property uchar green" << "\n"
			<< "property uchar blue" << "\n"
			<< "property float nx" << "\n"
			<< "property float ny" << "\n"
			<< "property float nz" << "\n"
			<< "end_header" << "\n";
		outFile.write((char*)ss.str().c_str(), ss.str().size());
		
		if (saveBinary) {
			float tp[3];
			uint8_t tc[3];
			for (int i = 0; i < n; ++i)
			{
				tp[0] = cloud->at(i).x;
				tp[1] = cloud->at(i).y;
				tp[2] = cloud->at(i).z;
				outFile.write((char*)tp, sizeof(float) * 3);
				tc[0] = cloud->at(i).r;
				tc[1] = cloud->at(i).g;
				tc[2] = cloud->at(i).b;
				outFile.write((char*)tc, sizeof(uint8_t) * 3);

				tp[0] = cloud->at(i).normal_x;
				tp[1] = cloud->at(i).normal_y;
				tp[2] = cloud->at(i).normal_z;
				outFile.write((char*)tp, sizeof(float) * 3);
				//outFile << "\n";
			}
		}
		else {
			for (int i = 0; i < n; ++i)
			{
				outFile << cloud->at(i).x << " "
					<< cloud->at(i).y << " "
					<< cloud->at(i).z << " "

					<< (int)(cloud->at(i).r) << " "
					<< (int)(cloud->at(i).g) << " "
					<< (int)(cloud->at(i).b) << " "

					<< cloud->at(i).normal_x << " "
					<< cloud->at(i).normal_y << " "
					<< cloud->at(i).normal_z
					<< "\n";
			}
		}

		outFile.close();
	}

	//write pointCloud ->.pcdFile
	EXPORT_API void write_point_cloud_in_pcd(pcl::PointCloud<pcl::PointXYZRGB>::Ptr cloud, const wchar_t* fileName, bool saveBinary)
	{
		int mode = std::ios::out;
		if (saveBinary)
			mode = std::ios::binary;
		std::ofstream outFile(fileName, mode);

		if (!outFile.is_open())
		{
			std::cerr << "Error! Can't open " << fileName << " for writing" << std::endl;
			return;
		}
		std::ostringstream ss;
		int n = cloud->size();
		// header with number of points
		ss << "# .PCD v.7 - Point Cloud Data file format\n"
			<< "VERSION .7\n"
			<< "FIELDS x y z rgb\n"
			<< "SIZE 4 4 4 4\n"
			<< "TYPE F F F U\n"
			<< "COUNT 1 1 1 1\n"
			<< "WIDTH " << n << "\n"
			<< "HEIGHT 1\n"
			<< "VIEWPOINT 0 0 0 1 0 0 0\n"
			<< "POINTS " << n << "\n";
		//
		if (saveBinary)
			ss << "DATA binary\n";
		else
			ss << "DATA ascii\n";

		outFile.write((char*)ss.str().c_str(), ss.str().size());




		int r, g, b;

		float tp[3];
		if (saveBinary) {
			for (int i = 0; i < n; ++i)
			{
				tp[0] = cloud->at(i).x;
				tp[1] = cloud->at(i).y;
				tp[2] = cloud->at(i).z;
				outFile.write((char*)tp, sizeof(float) * 3);
				b = cloud->at(i).b;
				g = cloud->at(i).g;
				r = cloud->at(i).r;

				unsigned int rgb = ((r << 16) | (g << 8) | (b << 0));
				outFile.write((char*)&(rgb), sizeof(unsigned int));
			}
		}
		else {
			for (int i = 0; i < n; ++i)
			{
				// position
				outFile << cloud->at(i).x << " "
					<< cloud->at(i).y << " "
					<< cloud->at(i).z << " ";
				// color(BGR)
				b = cloud->at(i).b;
				g = cloud->at(i).g;
				r = cloud->at(i).r;

				unsigned int rgb = ((r << 16) | (g << 8) | (b << 0));
				outFile << rgb <<"\n";
			}
		}

		outFile.close();
	}

	//write pointCloud ->.pcdFile with normal
	EXPORT_API void write_point_cloud_in_pcd_with_normal(pcl::PointCloud<pcl::PointXYZRGBNormal>::Ptr cloud, const wchar_t* fileName, bool saveBinary)
	{
		int mode = std::ios::out;
		if (saveBinary)
			mode = std::ios::binary;
		std::ofstream outFile(fileName, mode);

		if (!outFile.is_open())
		{
			std::cerr << "Error! Can't open " << fileName << " for writing" << std::endl;
			return;
		}

		std::ostringstream ss;
		//auto cloud = maincloud;
		int n = cloud->size();
		// header with number of points
		ss << "# .PCD v.7 - Point Cloud Data file format\n"
			<< "VERSION .7\n"
			<< "FIELDS x y z rgb normal_x normal_y normal_z\n"
			<< "SIZE 4 4 4 4 4 4 4\n"
			<< "TYPE F F F U F F F\n"
			<< "COUNT 1 1 1 1 1 1 1\n"
			<< "WIDTH " << n << "\n"
			<< "HEIGHT 1\n"
			<< "VIEWPOINT 0 0 0 1 0 0 0\n"
			<< "POINTS " << n << "\n";
		if (saveBinary)
			ss << "DATA binary\n";
		else
			ss << "DATA ascii\n";

		outFile.write((char*)ss.str().c_str(), ss.str().size());

		int r, g, b;

		float tp[3];

		if (saveBinary) {
			for (int i = 0; i < n; ++i)
			{
				tp[0] = cloud->at(i).x;
				tp[1] = cloud->at(i).y;
				tp[2] = cloud->at(i).z;
				outFile.write((char*)tp, sizeof(float) * 3);
				b = cloud->at(i).b;
				g = cloud->at(i).g;
				r = cloud->at(i).r;

				unsigned int rgb = ((r << 16) | (g << 8) | (b << 0));
				outFile.write((char*)&rgb, sizeof(unsigned int));

				tp[0] = cloud->at(i).normal_x;
				tp[1] = cloud->at(i).normal_y;
				tp[2] = cloud->at(i).normal_z;
				outFile.write((char*)tp, sizeof(float) * 3);
				
			}
		}
		else {
			for (int i = 0; i < n; ++i)
			{
				// position
				outFile << cloud->at(i).x << " "
					<< cloud->at(i).y << " "
					<< cloud->at(i).z << " ";

				// color(BGR)
				b = cloud->at(i).b;
				g = cloud->at(i).g;
				r = cloud->at(i).r;

				unsigned int rgb = ((r << 16) | (g << 8) | (b << 0));
				outFile << rgb << " ";
				outFile << cloud->at(i).normal_x << " "
					<< cloud->at(i).normal_y << " "
					<< cloud->at(i).normal_z << "\n";
			}
		}
		outFile.close();
	}



	//load CloudFile
	bool loadCloudFile(const wchar_t* fileName, pcl::PointCloud<pcl::PointXYZRGBNormal>::Ptr cloud) {
		int fileNameLen = (int)lstrlenW(fileName);
		int extPosition = fileNameLen - 4;
		std::string stra;
#ifdef _WIN32
		int len = WideCharToMultiByte(CP_ACP, 0, fileName, -1, NULL, 0, NULL, NULL);		
		stra.resize(len);
		WideCharToMultiByte(CP_ACP, 0, fileName, -1, &stra[0], len, NULL, NULL);
#endif

		if (lstrcmpW(fileName + extPosition, L".pcd") == 0) {
			pcl::PCDReader reader;			
			reader.read(stra, *cloud);
			if (cloud->empty()) {
				return false;
			} 


		}
		else if (lstrcmpW(fileName + extPosition, L".ply") == 0) {
			pcl::PLYReader reader;
			reader.read(stra, *cloud);


			if (cloud->empty()) {
				return false;
			}
		}
		else {
			return false;
		}
		
		return true;


	}


	//save CloudFIle
	EXPORT_API bool SaveCloudFile(const wchar_t* fileName, bool saveBinary, int size,float* x,float* y,float* z,int colorSize=0,float* r = nullptr,float* g = nullptr,float* b = nullptr,
		int normalSize=0,float* nx = nullptr,float* ny = nullptr, float* nz=nullptr) {
		int fileNameLen = (int)lstrlenW(fileName);
		int extPosition = fileNameLen - 4;

		//withoutNormal
		if (normalSize == 0) {
			pcl::PointCloud<pcl::PointXYZRGB>::Ptr cloud(new pcl::PointCloud<pcl::PointXYZRGB>);
			cloud->resize(size);
			for (int i = 0; i < size; ++i) {
				(*cloud)[i].x = x[i];
				(*cloud)[i].y = y[i];
				(*cloud)[i].z = z[i];
				if (colorSize != 0) {
					(*cloud)[i].r = r[i];
					(*cloud)[i].g = g[i];
					(*cloud)[i].b = b[i];
				}
			}
			if (lstrcmpW(fileName + extPosition, L".pcd") == 0) {
				write_point_cloud_in_pcd(cloud, fileName,  saveBinary);
				return true;
			}
			else if (lstrcmpW(fileName + extPosition, L".ply") == 0) {
				write_point_cloud_in_ply(cloud, fileName, saveBinary);
				return true;
			}
			else {
				return false;
			}
		}
		//withNormal
		else {
			pcl::PointCloud<pcl::PointXYZRGBNormal>::Ptr cloud(new pcl::PointCloud<pcl::PointXYZRGBNormal>);
			cloud->resize(size);
			for (int i = 0; i < size; ++i) {
				(*cloud)[i].x = x[i];
				(*cloud)[i].y = y[i];
				(*cloud)[i].z = z[i];
				if (colorSize != 0) {
					(*cloud)[i].r = r[i];
					(*cloud)[i].g = g[i];
					(*cloud)[i].b = b[i];
				}
				if (normalSize != 0) {
					(*cloud)[i].normal_x = nx[i];
					(*cloud)[i].normal_y = ny[i];
					(*cloud)[i].normal_z = nz[i];
				}
			}
			if (lstrcmpW(fileName + extPosition, L".pcd") == 0) {
				write_point_cloud_in_pcd_with_normal(cloud, fileName, saveBinary);
				return true;
			}
			else if (lstrcmpW(fileName + extPosition, L".ply") == 0) {
				write_point_cloud_in_ply_with_normal(cloud, fileName, saveBinary);
				return true;
			}
			else {
				return false;
			}
		}


	}
	//Cloud to Unity3D
	EXPORT_API bool GetCloudAndColorAndNormal(const wchar_t* fileName,int* resultVertLength, float** resultVertsX, float** resultVertsY, float** resultVertsZ,
		int* resultColorSize, float** resultColorR, float** resultColorG, float** resultColorB,
		int* resultNormalSize, float** resultNormalX, float** resultNormalY, float** resultNormalZ,float* avgDist)
	{
		pcl::PointCloud<pcl::PointXYZRGBNormal>::Ptr maincloud(new pcl::PointCloud<pcl::PointXYZRGBNormal>);
		loadCloudFile(fileName, maincloud);

		float* rVertsX = new float[maincloud->points.size()];
		float* rVertsY = new float[maincloud->points.size()];
		float* rVertsZ = new float[maincloud->points.size()];
		

		float* rCOlorR = new float[maincloud->points.size()];
		float* rCOlorG = new float[maincloud->points.size()];
		float* rCOlorB = new float[maincloud->points.size()];

		float val = sqrt(maincloud->points[0].normal_x*maincloud->points[0].normal_x + maincloud->points[0].normal_y*maincloud->points[0].normal_y + maincloud->points[0].normal_z*maincloud->points[0].normal_z);
		int normalSize = (int)(maincloud->points.size());
		if (val < 0.1) {
			normalSize = 0;
		}

		float* rNormalX = new float[normalSize];
		float* rNormalY = new float[normalSize];
		float* rNormalZ = new float[normalSize];
		
		for (size_t i = 0; i < maincloud->points.size(); ++i)
		{
			rVertsX[i] = maincloud->points[i].x;
			rVertsY[i] = maincloud->points[i].y;
			rVertsZ[i] = maincloud->points[i].z;


			rCOlorR[i] = maincloud->points[i].r;
			rCOlorG[i] = maincloud->points[i].g;
			rCOlorB[i] = maincloud->points[i].b;
			if (i < normalSize) {
				rNormalX[i] = maincloud->points[i].normal_x;
				rNormalY[i] = maincloud->points[i].normal_y;
				rNormalZ[i] = maincloud->points[i].normal_z;
			}
		}

		*resultVertsX = rVertsX;
		*resultVertsY = rVertsY;
		*resultVertsZ = rVertsZ;

		*resultColorSize = (int)(maincloud->points.size());
		*resultColorR = rCOlorR;
		*resultColorG = rCOlorG;
		*resultColorB = rCOlorB;

		*resultNormalSize = normalSize;
		*resultNormalX = rNormalX;
		*resultNormalY = rNormalY;
		*resultNormalZ = rNormalZ;

		int rVertLength = (int)(maincloud->points.size());
		*resultVertLength = rVertLength;
		pcl::PointCloud<pcl::PointXYZ>::Ptr cloud(new pcl::PointCloud<pcl::PointXYZ>);
		copyPointCloud(*maincloud, *cloud);
		*avgDist = CalcAvgDistanse(cloud);
		return true;
	}

	//txt File Read
	void GetCamParam(const wchar_t* KFileName, const wchar_t* camPoseName, Eigen::Matrix3d &K, Eigen::Matrix4d &campose) {
		std::ifstream inFile(KFileName);
		K.Identity();
		campose.Identity();
		for (int i = 0; i < 3; ++i)
		{
			for (int j = 0; j < 3; ++j)
			{
				inFile >> K.data()[j * 3 + i];
			}
		}
		inFile.close();

		inFile.open(camPoseName);
		for (int i = 0; i < 4; ++i)
		{
			for (int j = 0; j < 4; ++j)
			{
				inFile >> campose.data()[j * 4 + i];
			}
		}
		inFile.close();
	}

	//Depth to Point Cloud
	float ReconstFromDepth(cv::Mat* depthMap, const wchar_t* depthFileName, const wchar_t* KFileName, const wchar_t* camPoseName,
		std::vector<float> &xArray, std::vector<float> &yArray, std::vector<float> &zArray,
		bool useGetColor = false, cv::Mat* colorMap = nullptr, const wchar_t* textureFIleName = nullptr,
		std::vector<float>* rArray = nullptr, std::vector<float>* gArray = nullptr, std::vector<float>* bArray = nullptr,
		bool useCalcNormal = false, int normalCalcSize = 0,
		std::vector<float>* nxArray = nullptr, std::vector<float>* nyArray = nullptr, std::vector<float>* nzArray = nullptr,
		pcl::PointCloud<pcl::PointXYZ>::Ptr cloud = nullptr, bool useMultiThread = false
		) {

		Eigen::Matrix3d K;
		Eigen::Matrix4d campose;
		GetCamParam(KFileName, camPoseName, K, campose);

		*depthMap = ReadImage(depthFileName, cv::IMREAD_UNCHANGED);

		if (useGetColor) {
			*colorMap = ReadImage(textureFIleName, cv::IMREAD_COLOR);
		}
		//read
		int threadSize = std::thread::hardware_concurrency();
		std::vector< std::thread> threads(threadSize);
		std::mutex mtx;
		int sum = 0;
		int syncBlock = 0;
		auto lambda = [&](int tid, int threadNum) {
			int start = (int)((tid / (float)threadNum)*depthMap->rows);
			int end = (int)(((tid + 1) / (float)threadNum)*depthMap->rows);
			for (int y = start; y < end; ++y) {
				for (int x = 0; x < depthMap->cols; ++x) {
					auto zV = (depthMap->ptr<ushort>(y, x));
					if (*zV > 100) {
						Eigen::Vector3d pt2d(x, y, 1);
						Eigen::Vector3d pt3d;
						Eigen::Matrix3d pos2;
						pos2 = Eigen::Matrix3d::Identity();
						Eigen::Matrix3d posR;
						for (int i = 0; i < 3; ++i)
						{
							for (int j = 0; j < 3; ++j)
							{
								posR.data()[j * 3 + i] = campose.data()[j * 4 + i];
							}
						}
						Eigen::Vector3d posT;
						for (int i = 0; i < 3; ++i) {
							posT.data()[i] = campose.data()[3 * 4 + i];
						}

						pt3d = K.inverse()*pt2d;


						pt3d *= (*zV)*0.001;

						Eigen::Vector4d pt4d(pt3d.x(), pt3d.y(), pt3d.z(), 1);
						pt4d.y() = -pt4d.y();
						pt4d.z() = -pt4d.z();
						pt4d = campose*pt4d;
						mtx.lock();
						xArray.push_back(pt4d.x());
						yArray.push_back(pt4d.y());
						zArray.push_back(pt4d.z());
						mtx.unlock();
						pcl::PointXYZ newpt(pt4d.x(), pt4d.y(), pt4d.z());
						if (cloud != nullptr) {
							mtx.lock();
							cloud->push_back(newpt);
							mtx.unlock();
						}
						if (useGetColor) {
							auto color = colorMap->at<cv::Vec3b>(y, x);
							mtx.lock();
							bArray->push_back(color[0]);
							gArray->push_back(color[1]);
							rArray->push_back(color[2]);
							mtx.unlock();
						}
						if (useCalcNormal) {
							float dzdx = 0;
							float dzdy = 0;
							if (x >= normalCalcSize && y >= normalCalcSize && x < depthMap->cols - normalCalcSize && y < depthMap->rows - normalCalcSize) {
								for (int i = normalCalcSize; i > 0; --i) {
									ushort y1 = depthMap->at<ushort>(y + (-normalCalcSize + i), x);
									ushort y2 = depthMap->at<ushort>(y + i, x);
									ushort x2 = depthMap->at<ushort>(y, x + (-normalCalcSize + i));
									ushort x1 = depthMap->at<ushort>(y, x + i);

									dzdx += (x1 - x2);
									dzdy += (y1 - y2);
								}
							}
							dzdx /= normalCalcSize * 2;
							dzdy /= normalCalcSize * 2;


							Eigen::Vector3d pt2dn(dzdx, dzdy, -1.0f);

							pt2dn.normalize();
							mtx.lock();
							nxArray->push_back(pt2dn.x());
							nyArray->push_back(pt2dn.y());
							nzArray->push_back(-pt2dn.z());
							mtx.unlock();
						}
					}

				}

			}
		};


		if (useMultiThread) {
			for (int i = 0; i < threadSize; ++i) {
				sum += (i + 1);
			}
			for (int i = 0; i < threadSize; ++i) {
				threads[i] = std::thread(lambda, i, threadSize);
			}
			for (int i = 0; i < threadSize; ++i) {
				threads[i].join();
			}
		}
		else {
			lambda(0, 1);
		}
		if (cloud != nullptr) {
			return CalcAvgDistanse(cloud);
		}
		else {
			return 0;
		}
	}

	//16Bit Depth Image to Point Cloud without Texture
	EXPORT_API bool LoadPointCloudAndNormalFromDepth(uchar** depthImagePtr, int* depthSize,const wchar_t* depthFileName, const wchar_t* KFileName, const wchar_t* camPoseName,
		float** resultVertsX, float** resultVertsY, float** resultVertsZ, 
		float** resultNormalX, float** resultNormalY, float** resultNormalZ,
		int* resultVertLength, int normalCalcSize,float* avgDist,bool useMultiThread) {

		std::vector<float> xArray;
		std::vector<float> yArray;
		std::vector<float> zArray;
		std::vector<float> nxArray;
		std::vector<float> nyArray;
		std::vector<float> nzArray;
		cv::Mat depthImage;
		*avgDist = ReconstFromDepth(&depthImage,depthFileName, KFileName, camPoseName, xArray, yArray, zArray,false,nullptr,nullptr,nullptr,nullptr,nullptr,true, normalCalcSize,&nxArray,&nyArray,&nzArray,nullptr, useMultiThread);


		*resultVertLength = (int)(xArray.size());

		*resultVertsX = new float[*resultVertLength];
		*resultVertsY = new float[*resultVertLength];
		*resultVertsZ = new float[*resultVertLength];

		*resultNormalX = new float[*resultVertLength];
		*resultNormalY = new float[*resultVertLength];
		*resultNormalZ = new float[*resultVertLength];

		memcpy(*resultVertsX, &xArray[0], sizeof(float)*(*resultVertLength));
		memcpy(*resultVertsY, &yArray[0], sizeof(float)*(*resultVertLength));
		memcpy(*resultVertsZ, &zArray[0], sizeof(float)*(*resultVertLength));

		memcpy(*resultNormalX, &nxArray[0], sizeof(float)*(*resultVertLength));
		memcpy(*resultNormalY, &nyArray[0], sizeof(float)*(*resultVertLength));
		memcpy(*resultNormalZ, &nzArray[0], sizeof(float)*(*resultVertLength));


		std::vector<uchar> buf;
		cv::imencode(".png", depthImage, buf);
		*depthSize = (int)(buf.size());
		*depthImagePtr = new uchar[*depthSize];
		memcpy(*depthImagePtr, &buf[0], *depthSize);
		return true;
	}

	//16Bit Depth Image to Point Cloud with Texture
	EXPORT_API bool LoadPointCloudAndColorNormalFromDepth(
		uchar** depthImagePtr,int* depthSize, 
		uchar** textureImagePtr, int* textureSize,
		const wchar_t* depthFileName, const wchar_t* KFileName, const wchar_t* camPoseName, const wchar_t* textureFIleName,
		float** resultVertsX, float** resultVertsY, float** resultVertsZ, 
		float** resultColorR, float** resultColorG, float** resultColorB,
		float** resultNormalX, float** resultNormalY, float** resultNormalZ,
		int* resultVertLength, int normalCalcSize, float* avgDist) {
		std::vector<float> xArray;
		std::vector<float> yArray;
		std::vector<float> zArray;

		std::vector<float> rArray;
		std::vector<float> gArray;
		std::vector<float> bArray;

		std::vector<float> nxArray;
		std::vector<float> nyArray;
		std::vector<float> nzArray;
		pcl::PointCloud<pcl::PointXYZ>::Ptr cloud(new pcl::PointCloud<pcl::PointXYZ>);
		cv::Mat depthImage ;
		cv::Mat textureImage ;
		ReconstFromDepth(&depthImage,depthFileName, KFileName, camPoseName, xArray, yArray, zArray, true, &textureImage, textureFIleName, &rArray, &gArray, &bArray, true, normalCalcSize, &nxArray, &nyArray, &nzArray,cloud);
		
		*resultVertLength = (int)(xArray.size());

		*resultVertsX = new float[*resultVertLength];
		*resultVertsY = new float[*resultVertLength];
		*resultVertsZ = new float[*resultVertLength];
		memcpy(*resultVertsX, &xArray[0], sizeof(float)*(*resultVertLength));
		memcpy(*resultVertsY, &yArray[0], sizeof(float)*(*resultVertLength));
		memcpy(*resultVertsZ, &zArray[0], sizeof(float)*(*resultVertLength));


		*resultColorR = new float[*resultVertLength];
		*resultColorG = new float[*resultVertLength];
		*resultColorB = new float[*resultVertLength];
		memcpy(*resultColorR, &rArray[0], sizeof(float)*(*resultVertLength));
		memcpy(*resultColorG, &gArray[0], sizeof(float)*(*resultVertLength));
		memcpy(*resultColorB, &bArray[0], sizeof(float)*(*resultVertLength));

		*resultNormalX = new float[*resultVertLength];
		*resultNormalY = new float[*resultVertLength];
		*resultNormalZ = new float[*resultVertLength];
		memcpy(*resultNormalX, &nxArray[0], sizeof(float)*(*resultVertLength));
		memcpy(*resultNormalY, &nyArray[0], sizeof(float)*(*resultVertLength));
		memcpy(*resultNormalZ, &nzArray[0], sizeof(float)*(*resultVertLength));
		
		*avgDist = CalcAvgDistanse(cloud);
		memcpy(*resultVertsX, &xArray[0], sizeof(float)*(*resultVertLength));
		std::vector<uchar> buf;
		
		cv::imencode(".png", depthImage,buf);
		*depthSize = (int)(buf.size());
		*depthImagePtr = new uchar[buf.size()];
		memcpy(*depthImagePtr, &(buf[0]), buf.size());

		cv::imencode(".png", textureImage, buf);
		*textureSize = (int)(buf.size());
		*textureImagePtr = new uchar[buf.size()];
		memcpy(*textureImagePtr, &(buf[0]), buf.size());


		return true;
	}

	//8Bit Depth Image to Point Cloud
	EXPORT_API bool LoadPointCloudAndColorNormalFromDepthUseMinMax(uchar** depthImagePtr, int* depthSize,
		uchar** textureImagePtr, int* textureSize,
		const wchar_t* depthFileName, float zMin, float zMax,float scale, const wchar_t* textureFIleName,
		float** resultVertsX, float** resultVertsY, float** resultVertsZ, float** resultColorR, float** resultColorG, float** resultColorB,
		float** resultNormalX, float** resultNormalY, float** resultNormalZ,
		int* resultVertLength, int normalCalcSize,float* avgDist) {

		cv::Mat depthMap = ReadImage(depthFileName, cv::IMREAD_ANYDEPTH);
		cv::Mat colorMap;
		if(textureFIleName!=nullptr)
			colorMap = ReadImage(textureFIleName, cv::IMREAD_COLOR);

		std::vector<float> xArray;
		std::vector<float> yArray;
		std::vector<float> zArray;

		std::vector<float> rArray;
		std::vector<float> gArray;
		std::vector<float> bArray;

		std::vector<float> nxArray;
		std::vector<float> nyArray;
		std::vector<float> nzArray;
		uchar vmin = 255;
		uchar vmax = 0;

		for (int y = 0; y < depthMap.rows; ++y) {
			for (int x = 0; x < depthMap.cols; ++x) {
				auto zV = (depthMap.ptr<uchar>(y, x));
				if ((*zV) > 0) {
					if (vmin > (*zV)) {
						vmin = (*zV);
					}
					if (vmax < (*zV)) {
						vmax = (*zV);
					}
				}
			}
		}
		
		pcl::PointCloud<pcl::PointXYZ>::Ptr cloud(new pcl::PointCloud<pcl::PointXYZ>);
		for (int y = 0; y < depthMap.rows; ++y) {
			for (int x = 0; x < depthMap.cols; ++x) {
				auto zV = (depthMap.ptr<uchar>(y, x));
				if ((*zV) > 0) {
					Eigen::Vector3d pt3d;

					pt3d[0] = ((float)(x - depthMap.cols / 2) / (float)depthMap.cols )* scale;
					pt3d[1] = -((float)(y - depthMap.rows/2) / (float)depthMap.rows )* scale;
					pt3d[2] = -(((float)(*zV-vmin)/ (float)(vmax-vmin) ) * (zMax-zMin)+zMin);
					xArray.push_back(pt3d.x());
					yArray.push_back(pt3d.y());
					zArray.push_back(pt3d.z());
					pcl::PointXYZ newpt(pt3d.x(), pt3d.y(), pt3d.z());
					cloud->push_back(newpt);
					if (colorMap.rows > 0) {
						auto color = colorMap.at<cv::Vec3b>(y, x);
						bArray.push_back(color[0]);
						gArray.push_back(color[1]);
						rArray.push_back(color[2]);
					}
					else {
						bArray.push_back(255);
						gArray.push_back(255);
						rArray.push_back(255);
					}
					float dzdx = 0;
					float dzdy = 0;

					if (x >= normalCalcSize && y >= normalCalcSize && x < depthMap.cols - normalCalcSize && y < depthMap.rows - normalCalcSize) {
						for (int i = normalCalcSize; i > 0; --i) {
							char y1 = depthMap.at<char>(y + i, x);
							char y2 = depthMap.at<char>(y - i, x);
							char x1 = depthMap.at<char>(y, x + i);
							char x2 = depthMap.at<char>(y, x - i);
							dzdx += (x1 - x2);
							dzdy += (y1 - y2);
						}
						dzdx /= normalCalcSize * 2;
						dzdy /= normalCalcSize * 2;
					}

					Eigen::Vector3d pt2dn(dzdx, -dzdy, 1.0f);

					pt2dn.normalize();
					nxArray.push_back(pt2dn.x());
					nyArray.push_back(pt2dn.y());
					nzArray.push_back(pt2dn.z());

				}

			}

		}
		*resultVertLength = (int)(xArray.size());

		*resultVertsX = new float[*resultVertLength];
		*resultVertsY = new float[*resultVertLength];
		*resultVertsZ = new float[*resultVertLength];
		memcpy(*resultVertsX, &xArray[0], sizeof(float)*(*resultVertLength));
		memcpy(*resultVertsY, &yArray[0], sizeof(float)*(*resultVertLength));
		memcpy(*resultVertsZ, &zArray[0], sizeof(float)*(*resultVertLength));


		*resultColorR = new float[*resultVertLength];
		*resultColorG = new float[*resultVertLength];
		*resultColorB = new float[*resultVertLength];
		memcpy(*resultColorR, &rArray[0], sizeof(float)*(*resultVertLength));
		memcpy(*resultColorG, &gArray[0], sizeof(float)*(*resultVertLength));
		memcpy(*resultColorB, &bArray[0], sizeof(float)*(*resultVertLength));

		*resultNormalX = new float[*resultVertLength];
		*resultNormalY = new float[*resultVertLength];
		*resultNormalZ = new float[*resultVertLength];
		memcpy(*resultNormalX, &nxArray[0], sizeof(float)*(*resultVertLength));
		memcpy(*resultNormalY, &nyArray[0], sizeof(float)*(*resultVertLength));
		memcpy(*resultNormalZ, &nzArray[0], sizeof(float)*(*resultVertLength));

		*avgDist =  CalcAvgDistanse(cloud);
		

		std::vector<uchar> buf;

		cv::imencode(".png", depthMap, buf);
		*depthSize = (int)(buf.size());
		*depthImagePtr = new uchar[*depthSize];
		memcpy(*depthImagePtr, &(buf[0]), *depthSize);
		if (colorMap.rows > 0) {
			cv::imencode(".png", colorMap, buf);
			*textureSize = (int)(buf.size());
			*textureImagePtr = new uchar[*textureSize];
			memcpy(*textureImagePtr, &(buf[0]), *textureSize);
		}
		else {
			*textureSize = 0;
			*textureImagePtr = nullptr;
		}
		return true;


	}
	
	//Calulate PointCloud's Average Distanse
	float CalcAvgDistanse(pcl::PointCloud<pcl::PointXYZ>::Ptr cloud) {
		pcl::KdTreeFLANN<pcl::PointXYZ> kdtree;
		std::vector<int> pointIdx;
		std::vector< std::vector<float> > disTance;

		kdtree.setInputCloud(cloud);
		disTance.resize(cloud->size());
		for (int i = 0; i < cloud->size(); ++i) {
			kdtree.nearestKSearch(cloud->at(i), 5, pointIdx, disTance[i]);
		}
		float avgDist=0;
		for (int i = 0; i < disTance.size(); ++i) {
			avgDist += sqrt(disTance[i][1]);
		}
		avgDist /= disTance.size();
		return avgDist;
	}
	EXPORT_API float CalcAvgDistansePoints(float* pointsX, float* pointsY, float* pointsZ,int vertexSize){
		pcl::PointCloud<pcl::PointXYZ>::Ptr cloud(new pcl::PointCloud<pcl::PointXYZ>);
		for (int i = 0; i < vertexSize; ++i) {
			pcl::PointXYZ pts(pointsX[i], pointsY[i], pointsZ[i]);
			cloud->push_back(pts);
		}
		return CalcAvgDistanse(cloud);
	}

	//read txt
	bool read_intrinsic_param(const wchar_t* filename, Eigen::Matrix3d& K)
	{
		std::ifstream inFile(filename);

		inFile >> K(0, 0) >> K(0, 1) >> K(0, 2)
			>> K(1, 0) >> K(1, 1) >> K(1, 2)
			>> K(2, 0) >> K(2, 1) >> K(2, 2);

		inFile.close();

		std::cout << " K = \n" << K << std::endl;

		return true;
	}
	//read txt
	bool read_camera_pose(const wchar_t* filename, Eigen::Matrix4d& T)
	{
		std::ifstream inFile(filename);

		inFile
			>> T(0, 0) >> T(0, 1) >> T(0, 2) >> T(0, 3)
			>> T(1, 0) >> T(1, 1) >> T(1, 2) >> T(1, 3)
			>> T(2, 0) >> T(2, 1) >> T(2, 2) >> T(2, 3)
			>> T(3, 0) >> T(3, 1) >> T(3, 2) >> T(3, 3);

		inFile.close();

		std::cout << " Camera Pose = \n" << T << std::endl;

		return true;
	}



	//Calculate Mesh Texture's UV
	EXPORT_API int CalcTextureUV(char* MeshBuffer, const wchar_t* k_file, const wchar_t* campose_file, int width, int height, char** ResultMesh)
	{

		Eigen::Matrix3d K;
		read_intrinsic_param(k_file, K);

		Eigen::Matrix4d T;
		read_camera_pose(campose_file, T);

		Eigen::MatrixXd P(3, 4);
		P = T.inverse().block<3, 4>(0, 0);
		P = K*P;

		std::cout << "P=\n" << P << std::endl;

		mg::TriMesh_f mesh;
		mesh.loadMeshFromUNITY3D(MeshBuffer);

		// Assign texture coordinates
		int numVertices = mesh.numVertices();
		mesh.texCoor_.resize(numVertices);

		for (int i = 0; i < numVertices; ++i)
		{
			mg::Vector3f v = mesh.vertex_[i];

			Eigen::Vector4d x(v.x, -v.y, v.z, 1.0);

			Eigen::Vector3d q = P*x;

			float p[2];
			p[0] = q(0) / q(2);
			p[1] = q(1) / q(2);


			p[0] /= width;
			p[1] = 1.0 - p[1] / height;

			mesh.texCoor_[i] = mg::Vector2f(p[0], p[1]);
		}

		int numTri = mesh.numTriangles();
		mesh.tri_tex_.resize(numTri);
		for (int i = 0; i < numTri; ++i)
		{
			mg::Vector3i t = mesh.triangle_[i];
			mesh.tri_tex_[i] = t;
		}

		bool hasNormal = true;
		if (mesh.numVertexNormals() == 0) hasNormal = false;
		char* reBufferptr = 0;
		char** reBuffer = &reBufferptr;
		int num = mesh.writeMeshToUNITY3D(reBuffer);

		(*ResultMesh) = (*reBuffer);
		return num;
	}

	//save depth Image 8&16bits
	EXPORT_API bool SaveBothDepth(float* data, int width, int height, wchar_t* file8bitName, wchar_t* file16bitName) {
		cv::Mat mat(cv::Size(width, height), CV_16UC1);

		for (int i = 0; i < width; ++i) {
			for (int j = 0; j < height; ++j) {
				float value1 = (data[i + j*width] * 1000.0f);
				if (value1 > 100) {
					printf("breeakPoint");
				}
				mat.at<ushort>(j, i) = (ushort)(value1);
			}
		}
		cv::flip(mat, mat, 0);
		WriteImage(file16bitName, mat);

		float fmax, fmin;
		if (width*height > 0)
		{
			fmin = std::numeric_limits<float>::max();
			fmax = std::numeric_limits<float>::min();
		}
		for (int i = 0; i < width*height; ++i)
		{
			if (data[i] == std::numeric_limits<float>::infinity())
				continue;
			if (data[i] > 0.0f && data[i] < fmin)    fmin = data[i];
			if (data[i] > 0.0f && data[i] > fmax)    fmax = data[i];
		}
		mat = cv::Mat(cv::Size(width, height), CV_8UC1);
		for (int i = 0; i < width; ++i) {
			for (int j = 0; j < height; ++j) {
				if (data[i + j*width] > 0 && data[i + j*width] < std::numeric_limits<float>::infinity()) {
					uchar value= (unsigned char)((254.999f*(((data[i + j*width]) - fmin) / (fmax - fmin)))) + 1;
					mat.at<uchar>(j, i) = value;
				}
				else {
					mat.at<uchar>(j, i) = 0;
				}
			}
		}
		cv::flip(mat, mat, 0);
		WriteImage(file8bitName, mat);
		return true;
	}


} // end of export C block