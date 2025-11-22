```text
# UnityGitPack
1.申领Token步骤
	1)Resource owner
		选择用户/组织
	2)Repository access
		选择"All repositories"
		而非"Public repositories",这个是只读访问任意公共仓库。
	3)Permissions(最小权限) 点击 Add repository permissions，选择以下权限：
		(1)Administration → Read and write(强制需要)
			用于:
			创建仓库
			修改仓库设置(必要)
			删除仓库(不一定需要，但“创建仓库”必须要这个)
		(2)Contents → Read and write(强制需要)
			用于:
			git push 上传/修改文件 创建分支
			这是发布 .unitypackage 时实际写文件到仓库所需权限。
		(3) Metadata → Read-only(默认 无法操作)
			用于：
			获取仓库信息(不涉及隐私)
			GitHub API 使用它读取 repo 信息。
		(4) Actions → Read and write(可选)
			用于:
			发布 Release
			上传 Release 附件文件

2.使用ScriptableObject
	1) Project右键Create->Tools->MomosUnityGitPack->Create User Git Settings
		切记不要把这个提交了,会暴露token!!!
	2) 填写GitData
		// 如此填写,设计中不希望用户整个提交Assets目录。
		Local Package Path Of Assets: Scripts/xxPack
	3) 填写UpmData
		Name: com.xxx.xxx //需符合反向域名规范
		Version: x.y.z
		Unity: 版本号
```